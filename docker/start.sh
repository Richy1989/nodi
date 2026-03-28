#!/bin/sh
# Entrypoint for the combined nodi container.
# Applies default port values and then hands off to supervisord.

set -e

# Apply defaults if not set by the caller (docker run -e / Unraid UI)
export CORE_PORT="${CORE_PORT:-5100}"
export WEB_PORT="${WEB_PORT:-8080}"

echo "Starting nodi"
echo "  Core (internal) : http://localhost:$CORE_PORT"
echo "  Web  (exposed)  : http://+:$WEB_PORT"

# supervisord reads %(ENV_CORE_PORT)s and %(ENV_WEB_PORT)s from the environment
exec /usr/bin/supervisord -n -c /etc/supervisor/supervisord.conf
