# Mystira Chain Infrastructure Module
# Terraform module for deploying Mystira.Chain blockchain infrastructure

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

variable "chain_node_count" {
  description = "Number of chain nodes to deploy"
  type        = number
  default     = 3
}

variable "chain_node_instance_type" {
  description = "EC2 instance type for chain nodes"
  type        = string
  default     = "t3.medium"
}

variable "chain_storage_size" {
  description = "Storage size in GB for chain data"
  type        = number
  default     = 100
}

variable "vpc_id" {
  description = "VPC ID for chain deployment"
  type        = string
}

variable "subnet_ids" {
  description = "Subnet IDs for chain nodes"
  type        = list(string)
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}

locals {
  name_prefix = "mystira-chain-${var.environment}"
  common_tags = merge(var.tags, {
    Component   = "chain"
    Environment = var.environment
    ManagedBy   = "terraform"
  })
}

# Security Group for Chain Nodes
resource "aws_security_group" "chain_nodes" {
  name        = "${local.name_prefix}-nodes-sg"
  description = "Security group for Mystira Chain nodes"
  vpc_id      = var.vpc_id

  # P2P communication between chain nodes
  ingress {
    from_port   = 30303
    to_port     = 30303
    protocol    = "tcp"
    self        = true
    description = "Chain P2P TCP"
  }

  ingress {
    from_port   = 30303
    to_port     = 30303
    protocol    = "udp"
    self        = true
    description = "Chain P2P UDP"
  }

  # RPC endpoint (internal only)
  ingress {
    from_port   = 8545
    to_port     = 8545
    protocol    = "tcp"
    cidr_blocks = ["10.0.0.0/8"]
    description = "JSON-RPC HTTP"
  }

  # WebSocket endpoint (internal only)
  ingress {
    from_port   = 8546
    to_port     = 8546
    protocol    = "tcp"
    cidr_blocks = ["10.0.0.0/8"]
    description = "JSON-RPC WebSocket"
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
    description = "Allow all outbound"
  }

  tags = merge(local.common_tags, {
    Name = "${local.name_prefix}-nodes-sg"
  })
}

# EBS volumes for chain data persistence
resource "aws_ebs_volume" "chain_data" {
  count             = var.chain_node_count
  availability_zone = element(data.aws_availability_zones.available.names, count.index % length(data.aws_availability_zones.available.names))
  size              = var.chain_storage_size
  type              = "gp3"
  encrypted         = true

  tags = merge(local.common_tags, {
    Name = "${local.name_prefix}-data-${count.index}"
  })
}

data "aws_availability_zones" "available" {
  state = "available"
}

# IAM Role for Chain Nodes
resource "aws_iam_role" "chain_node" {
  name = "${local.name_prefix}-node-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "ec2.amazonaws.com"
        }
      }
    ]
  })

  tags = local.common_tags
}

resource "aws_iam_instance_profile" "chain_node" {
  name = "${local.name_prefix}-node-profile"
  role = aws_iam_role.chain_node.name
}

# CloudWatch Log Group for Chain Logs
resource "aws_cloudwatch_log_group" "chain" {
  name              = "/mystira/chain/${var.environment}"
  retention_in_days = var.environment == "prod" ? 90 : 30

  tags = local.common_tags
}

output "security_group_id" {
  description = "Security group ID for chain nodes"
  value       = aws_security_group.chain_nodes.id
}

output "iam_role_arn" {
  description = "IAM role ARN for chain nodes"
  value       = aws_iam_role.chain_node.arn
}

output "log_group_name" {
  description = "CloudWatch log group name"
  value       = aws_cloudwatch_log_group.chain.name
}

output "ebs_volume_ids" {
  description = "EBS volume IDs for chain data"
  value       = aws_ebs_volume.chain_data[*].id
}
