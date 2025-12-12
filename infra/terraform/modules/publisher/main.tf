# Mystira Publisher Infrastructure Module
# Terraform module for deploying Mystira.Publisher service infrastructure

terraform {
  required_version = ">= 1.5.0"
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "~> 2.23"
    }
  }
}

variable "environment" {
  description = "Deployment environment (dev, staging, prod)"
  type        = string
}

variable "publisher_replica_count" {
  description = "Number of publisher service replicas"
  type        = number
  default     = 2
}

variable "publisher_instance_type" {
  description = "EC2 instance type for publisher service"
  type        = string
  default     = "t3.small"
}

variable "vpc_id" {
  description = "VPC ID for publisher deployment"
  type        = string
}

variable "subnet_ids" {
  description = "Subnet IDs for publisher service"
  type        = list(string)
}

variable "chain_rpc_endpoint" {
  description = "RPC endpoint for Mystira Chain"
  type        = string
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}

locals {
  name_prefix = "mystira-publisher-${var.environment}"
  common_tags = merge(var.tags, {
    Component   = "publisher"
    Environment = var.environment
    ManagedBy   = "terraform"
  })
}

# Security Group for Publisher Service
resource "aws_security_group" "publisher" {
  name        = "${local.name_prefix}-sg"
  description = "Security group for Mystira Publisher service"
  vpc_id      = var.vpc_id

  # HTTP API endpoint
  ingress {
    from_port   = 3000
    to_port     = 3000
    protocol    = "tcp"
    cidr_blocks = ["10.0.0.0/8"]
    description = "Publisher API HTTP"
  }

  # Health check endpoint
  ingress {
    from_port   = 3001
    to_port     = 3001
    protocol    = "tcp"
    cidr_blocks = ["10.0.0.0/8"]
    description = "Health check endpoint"
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
    description = "Allow all outbound"
  }

  tags = merge(local.common_tags, {
    Name = "${local.name_prefix}-sg"
  })
}

# IAM Role for Publisher Service
resource "aws_iam_role" "publisher" {
  name = "${local.name_prefix}-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "ecs-tasks.amazonaws.com"
        }
      }
    ]
  })

  tags = local.common_tags
}

# IAM Policy for Publisher Service
resource "aws_iam_role_policy" "publisher" {
  name = "${local.name_prefix}-policy"
  role = aws_iam_role.publisher.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "secretsmanager:GetSecretValue",
          "ssm:GetParameter",
          "ssm:GetParameters"
        ]
        Resource = [
          "arn:aws:secretsmanager:*:*:secret:mystira/publisher/*",
          "arn:aws:ssm:*:*:parameter/mystira/publisher/*"
        ]
      },
      {
        Effect = "Allow"
        Action = [
          "logs:CreateLogStream",
          "logs:PutLogEvents"
        ]
        Resource = "${aws_cloudwatch_log_group.publisher.arn}:*"
      },
      {
        Effect = "Allow"
        Action = [
          "sqs:SendMessage",
          "sqs:ReceiveMessage",
          "sqs:DeleteMessage"
        ]
        Resource = aws_sqs_queue.publisher_events.arn
      }
    ]
  })
}

# SQS Queue for Publisher Events
resource "aws_sqs_queue" "publisher_events" {
  name                       = "${local.name_prefix}-events"
  visibility_timeout_seconds = 300
  message_retention_seconds  = 86400
  receive_wait_time_seconds  = 20

  redrive_policy = jsonencode({
    deadLetterTargetArn = aws_sqs_queue.publisher_dlq.arn
    maxReceiveCount     = 3
  })

  tags = local.common_tags
}

# Dead Letter Queue for failed events
resource "aws_sqs_queue" "publisher_dlq" {
  name                      = "${local.name_prefix}-events-dlq"
  message_retention_seconds = 1209600 # 14 days

  tags = local.common_tags
}

# CloudWatch Log Group for Publisher Logs
resource "aws_cloudwatch_log_group" "publisher" {
  name              = "/mystira/publisher/${var.environment}"
  retention_in_days = var.environment == "prod" ? 90 : 30

  tags = local.common_tags
}

# Parameter Store for Publisher Configuration
resource "aws_ssm_parameter" "chain_rpc_endpoint" {
  name        = "/mystira/publisher/${var.environment}/chain_rpc_endpoint"
  description = "Chain RPC endpoint for publisher service"
  type        = "SecureString"
  value       = var.chain_rpc_endpoint

  tags = local.common_tags
}

output "security_group_id" {
  description = "Security group ID for publisher service"
  value       = aws_security_group.publisher.id
}

output "iam_role_arn" {
  description = "IAM role ARN for publisher service"
  value       = aws_iam_role.publisher.arn
}

output "log_group_name" {
  description = "CloudWatch log group name"
  value       = aws_cloudwatch_log_group.publisher.name
}

output "sqs_queue_url" {
  description = "SQS queue URL for publisher events"
  value       = aws_sqs_queue.publisher_events.url
}

output "sqs_queue_arn" {
  description = "SQS queue ARN for publisher events"
  value       = aws_sqs_queue.publisher_events.arn
}
