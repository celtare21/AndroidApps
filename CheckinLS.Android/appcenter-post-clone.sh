#!/usr/bin/env bash

npm install node-jq --save

jq -n \
            --arg cs "$CONN_STR" \
            --arg an "$ANALYTICS_STR" \
            '{ConnStr: $cs, analytics: $an}' > $APPCENTER_SOURCE_DIRECTORY/CheckinLS/secrets.json