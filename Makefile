# Api – Makefile
# ===================
# dev       → lokale DB in K8s + Migrations + Hot-Reload
# clean     → Dev-Prozesse stoppen (DB bleibt persistent)
# db-clean  → DB-Daten komplett löschen (PVC weg)
# prod      → Docker bauen, pushen, prod deployen

.PHONY: dev clean db-clean prod status logs seq-logs redis-logs help \
        _db-apply _db-wait _db-forward _migrate \
        _seq-apply _seq-wait _seq-forward \
        _redis-apply _redis-wait _redis-forward

.DEFAULT_GOAL := help

# ---------------------------------------------------------------------------
# Konfiguration
# ---------------------------------------------------------------------------
NAMESPACE     := cmc-local
PROD_NS       := cmc-prod
IMAGE_NAME    := cmc-web
IMAGE_TAG     := local
REGISTRY      := ghcr.io/ebejay95/tool
DEV_PORT      := 8080
DB_PORT       := 5432
SEQ_PORT      := 5341
REDIS_PORT    := 6379
REDIS_PASS    := local_redis_password
PROJECT       := src/Api
DB_CONN       := "Host=localhost;Port=$(DB_PORT);Database=cmc_local;Username=cmc_user;Password=local_dev_password_change_me;SslMode=Disable"
REDIS_CONN    := "localhost:$(REDIS_PORT),password=$(REDIS_PASS)"

# ---------------------------------------------------------------------------
# dev: DB hochfahren (falls nötig), migrieren, Hot-Reload starten
# ---------------------------------------------------------------------------
dev: _db-apply _seq-apply _redis-apply _db-wait _seq-wait _redis-wait _db-forward _seq-forward _redis-forward _migrate
	@echo "🚀 Hot-Reload auf http://localhost:$(DEV_PORT) ..."
	@echo "📊 Seq (Logs/Traces): http://localhost:$(SEQ_PORT)"
	@lsof -ti:$(DEV_PORT) | xargs kill -9 2>/dev/null || true
	@cd $(PROJECT) && \
		ASPNETCORE_ENVIRONMENT=Development \
		ConnectionStrings__DefaultConnection=$(DB_CONN) \
		Redis__ConnectionString=$(REDIS_CONN) \
		dotnet watch run --urls "http://localhost:$(DEV_PORT)"

# ---------------------------------------------------------------------------
# clean: Dev-Prozesse stoppen – DB bleibt erhalten
# ---------------------------------------------------------------------------
clean:
	@echo "🛑 Stoppe Dev-Prozesse (DB + Seq + Redis bleiben)..."
	@pkill -f "dotnet watch" 2>/dev/null || true
	@pkill -f "kubectl.*port-forward.*$(DB_PORT)" 2>/dev/null || true
	@pkill -f "kubectl.*port-forward.*$(SEQ_PORT)" 2>/dev/null || true
	@pkill -f "kubectl.*port-forward.*$(REDIS_PORT)" 2>/dev/null || true
	@lsof -ti:$(DEV_PORT) | xargs kill -9 2>/dev/null || true
	@echo "✅ Gestoppt"

# ---------------------------------------------------------------------------
# db-clean: DB-Daten löschen → nächstes 'make dev' baut sie neu auf
# ---------------------------------------------------------------------------
db-clean: clean
	@echo "🗑️  Lösche DB-Daten (PVC + Deployment)..."
	@kubectl delete deployment postgres  -n $(NAMESPACE) --ignore-not-found=true
	@kubectl delete service   postgres   -n $(NAMESPACE) --ignore-not-found=true
	@kubectl delete pvc       postgres-data -n $(NAMESPACE) --ignore-not-found=true
	@echo "✅ DB gelöscht – 'make dev' baut sie neu auf"

# ---------------------------------------------------------------------------
# prod: Image bauen, pushen, prod-Namespace deployen
# ---------------------------------------------------------------------------
prod:
	@echo "🔨 Baue Docker Image $(REGISTRY)/$(IMAGE_NAME):latest ..."
	@docker build -t $(IMAGE_NAME):$(IMAGE_TAG) -t $(REGISTRY)/$(IMAGE_NAME):latest .
	@echo "📦 Pushe nach $(REGISTRY) ..."
	@docker push $(REGISTRY)/$(IMAGE_NAME):latest
	@echo "☸️  Deploye in Namespace $(PROD_NS) ..."
	@kubectl apply -f k8s/prod/
	@echo "✅ Prod-Deployment abgeschlossen"

# ---------------------------------------------------------------------------
# Hilfsbefehle
# ---------------------------------------------------------------------------
status:
	@kubectl get pods -n $(NAMESPACE)

logs:
	@kubectl logs -n $(NAMESPACE) -l app=postgres -f

seq-logs:
	@kubectl logs -n $(NAMESPACE) -l app=seq -f

redis-logs:
	@kubectl logs -n $(NAMESPACE) -l app=redis -f

help:
	@echo ""
	@echo "Api – Befehle:"
	@echo ""
	@echo "  dev        🚀 DB + Seq + Redis starten + migrieren + Hot-Reload"
	@echo "  clean      🛑 Dev-Prozesse stoppen  (DB/Seq/Redis bleiben persistent)"
	@echo "  db-clean   🗑️  DB-Daten löschen     (PVC weg, nächstes 'dev' baut neu)"
	@echo "  prod       🚀 Docker bauen + pushen + prod deployen"
	@echo "  status     📊 K8s Pod-Status (local)"
	@echo "  logs       📜 Postgres-Logs folgen"
	@echo "  seq-logs   📜 Seq-Logs folgen"
	@echo "  redis-logs 📜 Redis-Logs folgen"
	@echo ""
	@echo "  App:   http://localhost:$(DEV_PORT)"
	@echo "  Seq:   http://localhost:$(SEQ_PORT)"
	@echo "  Redis: localhost:$(REDIS_PORT)"
	@echo "  REGISTRY=$(REGISTRY)  (überschreibbar: make prod REGISTRY=...)"
	@echo ""

# ---------------------------------------------------------------------------
# Interne Targets (nicht direkt aufrufen)
# ---------------------------------------------------------------------------
_db-apply:
	@echo "☸️  Wende lokale DB-Ressourcen an..."
	@kubectl apply -f k8s/local/namespace.yaml
	@kubectl apply -f k8s/local/secrets.local.yaml
	@kubectl apply -f k8s/local/postgres.yaml

_seq-apply:
	@echo "☸️  Starte Seq (Logs/Traces)..."
	@kubectl apply -f k8s/local/seq.yaml

_seq-wait:
	@echo "⏳ Warte auf Seq-Pod..."
	@kubectl rollout status deployment/seq -n $(NAMESPACE) --timeout=300s

_seq-forward:
	@echo "📡 Port-Forward Seq → localhost:$(SEQ_PORT) ..."
	@pkill -f "kubectl.*port-forward.*$(SEQ_PORT)" 2>/dev/null || true
	@lsof -ti:$(SEQ_PORT) | xargs kill -9 2>/dev/null || true
	@kubectl port-forward -n $(NAMESPACE) service/seq $(SEQ_PORT):$(SEQ_PORT) &
	@sleep 2

_redis-apply:
	@echo "☸️  Starte Redis (SignalR-Backplane)..."
	@kubectl apply -f k8s/local/redis.yaml

_redis-wait:
	@echo "⏳ Warte auf Redis-Pod..."
	@kubectl rollout status deployment/redis -n $(NAMESPACE) --timeout=60s

_redis-forward:
	@echo "📡 Port-Forward Redis → localhost:$(REDIS_PORT) ..."
	@pkill -f "kubectl.*port-forward.*$(REDIS_PORT)" 2>/dev/null || true
	@lsof -ti:$(REDIS_PORT) | xargs kill -9 2>/dev/null || true
	@kubectl port-forward -n $(NAMESPACE) service/redis $(REDIS_PORT):$(REDIS_PORT) &
	@sleep 2

_db-wait:
	@echo "⏳ Warte auf Postgres-Pod..."
	@kubectl rollout status deployment/postgres -n $(NAMESPACE) --timeout=120s

_db-forward:
	@echo "📡 Port-Forward DB → localhost:$(DB_PORT) ..."
	@pkill -f "kubectl.*port-forward.*$(DB_PORT)" 2>/dev/null || true
	@lsof -ti:$(DB_PORT) | xargs kill -9 2>/dev/null || true
	@sleep 1
	@kubectl port-forward -n $(NAMESPACE) service/postgres $(DB_PORT):$(DB_PORT) &
	@sleep 3

_migrate:
	@echo "🔄 Führe EF-Migrationen aus..."
	@cd $(PROJECT) && \
		dotnet ef database update \
			--project ../Modules/Identity/Identity.Infrastructure \
			--startup-project . \
			--context IdentityDbContext \
			--connection $(DB_CONN) && \
		dotnet ef database update \
			--project ../Modules/Todos/Todos.Infrastructure \
			--startup-project . \
			--context TodosDbContext \
			--connection $(DB_CONN) && \
		dotnet ef database update \
			--project ../Modules/Notifications/Notifications.Infrastructure \
			--startup-project . \
			--context NotificationsDbContext \
			--connection $(DB_CONN)
	@echo "✅ Migrationen abgeschlossen"
