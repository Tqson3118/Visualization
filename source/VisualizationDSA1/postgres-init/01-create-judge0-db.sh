#!/bin/bash
set -e
# Tạo database riêng cho Judge0 để app DB (visualization_dsa_dev) luôn trống
# khi backend chạy EnsureCreatedAsync (EnsureCreated bỏ qua nếu DB có bảng từ trước).
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    CREATE DATABASE judge0;
EOSQL
