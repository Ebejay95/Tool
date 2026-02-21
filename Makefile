# Minimal local-only workflow (Docker Desktop Kubernetes)

KUBECTL      ?= kubectl
K8S_CONTEXT  ?= docker-desktop
NAMESPACE    ?= cmc-local
IMAGE        ?= cmc-web:local
KUBECTL_CMD  := $(KUBECTL) $(if $(K8S_CONTEXT),--context $(K8S_CONTEXT),)

all: help

help:
	@echo "Local dev (Docker Desktop Kubernetes)"
	@echo ""
	@echo "  make k8s-up           # build image + deploy (namespace/secrets/postgres/app)"
	@echo "  make k8s-seed         # seed master user"
	@echo "  make k8s-status       # show resources"
	@echo "  make k8s-logs         # follow app logs"
	@echo "  make k8s-port-forward # http://localhost:8080"
	@echo "  make k8s-down         # delete app + db"
	@echo ""
	@echo "Vars: K8S_CONTEXT=docker-desktop (override if needed), IMAGE=cmc-web:local"

k8s-check:
	@CTX=$$($(KUBECTL) config current-context 2>/dev/null || true); \
	if [ -z "$$CTX" ]; then \
		echo "❌ Kein kubectl context konfiguriert."; \
		echo "   In Docker Desktop: Kubernetes aktivieren und dann: kubectl config use-context docker-desktop"; \
		exit 2; \
	fi

image-build:
	@echo "🐋 Building image $(IMAGE)…"
	docker build -t $(IMAGE) .

k8s-up: k8s-check image-build
	@$(KUBECTL_CMD) apply -f k8s/local/namespace.yaml
	@$(KUBECTL_CMD) apply -f k8s/local/secrets.local.yaml
	@$(KUBECTL_CMD) apply -f k8s/local/postgres.yaml
	@$(KUBECTL_CMD) apply -f k8s/local/app.yaml
	@$(KUBECTL_CMD) rollout status -n $(NAMESPACE) deploy/cmc-app --timeout=180s

k8s-down: k8s-check
	@-$(KUBECTL_CMD) delete -f k8s/local/app.yaml --ignore-not-found
	@-$(KUBECTL_CMD) delete -f k8s/local/postgres.yaml --ignore-not-found

k8s-status: k8s-check
	@$(KUBECTL_CMD) get all -n $(NAMESPACE)

k8s-logs: k8s-check
	@$(KUBECTL_CMD) logs -n $(NAMESPACE) -f deploy/cmc-app --tail=200

k8s-port-forward: k8s-check
	@echo "http://localhost:8080"
	@$(KUBECTL_CMD) port-forward -n $(NAMESPACE) service/cmc-app 8080:80

k8s-seed: k8s-check
	@-$(KUBECTL_CMD) delete job -n $(NAMESPACE) seed-master-user --ignore-not-found
	@$(KUBECTL_CMD) apply -f k8s/local/seed-master-user.yaml
	@$(KUBECTL_CMD) wait -n $(NAMESPACE) --for=condition=complete job/seed-master-user --timeout=600s
	@$(KUBECTL_CMD) logs -n $(NAMESPACE) job/seed-master-user --tail=200

.PHONY: all help k8s-check image-build k8s-up k8s-down k8s-status k8s-logs k8s-port-forward k8s-seed
