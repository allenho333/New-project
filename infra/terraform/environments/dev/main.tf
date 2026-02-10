terraform {
  required_version = ">= 1.7.0"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

provider "aws" {
  region = var.aws_region
}

locals {
  project_name = "interview-showcase"
}

module "network" {
  source       = "../../modules/network"
  project_name = local.project_name
}

module "app" {
  source       = "../../modules/app"
  project_name = local.project_name
  image_tag    = "latest"
}
