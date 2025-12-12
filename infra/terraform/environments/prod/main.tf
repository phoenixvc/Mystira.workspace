# Mystira Production Environment
# Terraform configuration for production environment

terraform {
  required_version = ">= 1.5.0"

  backend "s3" {
    bucket         = "mystira-terraform-state"
    key            = "prod/terraform.tfstate"
    region         = "us-east-1"
    encrypt        = true
    dynamodb_table = "mystira-terraform-locks"
  }

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

provider "aws" {
  region = var.aws_region

  default_tags {
    tags = {
      Project     = "Mystira"
      Environment = "prod"
      ManagedBy   = "terraform"
    }
  }
}

variable "aws_region" {
  description = "AWS region for deployment"
  type        = string
  default     = "us-east-1"
}

variable "vpc_id" {
  description = "VPC ID for deployment"
  type        = string
}

variable "subnet_ids" {
  description = "Subnet IDs for deployment"
  type        = list(string)
}

# Chain Infrastructure
module "chain" {
  source = "../../modules/chain"

  environment              = "prod"
  chain_node_count         = 3
  chain_node_instance_type = "t3.large"
  chain_storage_size       = 500
  vpc_id                   = var.vpc_id
  subnet_ids               = var.subnet_ids

  tags = {
    CostCenter = "production"
    Critical   = "true"
  }
}

# Publisher Infrastructure
module "publisher" {
  source = "../../modules/publisher"

  environment             = "prod"
  publisher_replica_count = 3
  publisher_instance_type = "t3.medium"
  vpc_id                  = var.vpc_id
  subnet_ids              = var.subnet_ids
  chain_rpc_endpoint      = "http://chain.prod.mystira.internal:8545"

  tags = {
    CostCenter = "production"
    Critical   = "true"
  }
}

output "chain_security_group_id" {
  value = module.chain.security_group_id
}

output "publisher_security_group_id" {
  value = module.publisher.security_group_id
}

output "publisher_sqs_queue_url" {
  value = module.publisher.sqs_queue_url
}
