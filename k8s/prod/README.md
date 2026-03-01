# Production (k8s/prod)

This folder is a minimal example production setup.

## Prereqs

- Ingress controller (e.g. `ingress-nginx`) installed.
- `cert-manager` installed and a `ClusterIssuer` named `letsencrypt-prod` exists.
- Image available in GHCR (default manifest uses `ghcr.io/ebejay95/tool/cmc-web:latest`).

## Secrets

- Copy `secrets.prod.example.yaml` to `secrets.prod.yaml` and fill values.
- `secrets.prod.yaml` is git-ignored by `.gitignore`.

## Hostname

- Replace `cmc.example.com` in `app.yaml` and `ingress.yaml`.

## Apply

Use `make prod-up` from repo root.
