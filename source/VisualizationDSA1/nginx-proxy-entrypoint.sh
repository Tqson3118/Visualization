#!/bin/sh
# Entrypoint script for nginx-proxy to substitute environment variables

set -e

# Substitute environment variables in nginx config
envsubst '${DOMAIN}' < /etc/nginx/templates/default.conf.template > /etc/nginx/conf.d/default.conf

# Execute the original entrypoint
exec "$@"