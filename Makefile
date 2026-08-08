IMAGE      ?= mirzasaikatahmmed/saas-based-school-management-system-api
TAG        ?= latest
FULL_IMAGE := $(IMAGE):$(TAG)
DOCKERFILE ?= Dockerfile
COMPOSE    ?= docker compose

.PHONY: help build up down push pull logs restart ps

help:
	@echo "Targets:"
	@echo "  make build    - Build $(FULL_IMAGE)"
	@echo "  make up       - docker compose up -d"
	@echo "  make down     - docker compose down"
	@echo "  make push     - Push $(FULL_IMAGE) to Docker Hub"
	@echo "  make pull     - Pull $(FULL_IMAGE) from Docker Hub"
	@echo "  make logs     - Follow API container logs"
	@echo "  make restart  - build + recreate api service"
	@echo "  make ps       - Show compose services"

build:
	docker build -f $(DOCKERFILE) -t $(FULL_IMAGE) .

up:
	$(COMPOSE) up -d

down:
	$(COMPOSE) down

push:
	docker push $(FULL_IMAGE)

pull:
	docker pull $(FULL_IMAGE)

logs:
	$(COMPOSE) logs -f api

restart: build
	$(COMPOSE) up -d --force-recreate --no-deps api

ps:
	$(COMPOSE) ps
