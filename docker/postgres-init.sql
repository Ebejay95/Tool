-- Automatic database initialization
SELECT 'CREATE DATABASE cmc_dev' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'cmc_dev')\gexec
