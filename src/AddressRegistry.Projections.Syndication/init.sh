#!/bin/sh

CT=syndication

curl -s http://169.254.170.2/v2/metadata > metadata
cat metadata

CONTAINERID=$(cat metadata | jq -r ".Containers[] | select(.Labels[\"com.amazonaws.ecs.container-name\"] | startswith(\"basisregisters-\") and endswith(\"-$CT\")) | .DockerId")
echo CONTAINERID = $CONTAINERID

sed -i "s/REPLACE_CONTAINERID/$CONTAINERID/g" appsettings.json

./AddressRegistry.Projections.Syndication
