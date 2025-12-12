# Mystira Staging Environment
# Terraform configuration for staging environment

terraform {
  required_version = ">= 1.5.0"

  backend "s3" {
    bucket         = "mystira-terraform-state"
    key            = "staging/terraform.tfstate"
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
      Environment = "staging"
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

  environment              = "staging"
  chain_node_count         = 2
  chain_node_instance_type = "t3.medium"
  chain_storage_size       = 100
  vpc_id                   = var.vpc_id
  subnet_ids               = var.subnet_ids

  tags = {
    CostCenter = "staging"
  }
}

# Publisher Infrastructure
module "publisher" {
  source = "../../modules/publisher"

  environment             = "staging"
  publisher_replica_count = 2
  publisher_instance_type = "t3.small"
  vpc_id                  = var.vpc_id
  subnet_ids              = var.subnet_ids
  chain_rpc_endpoint      = "http://chain.staging.mystira.internal:8545"

  tags = {
    CostCenter = "staging"
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
