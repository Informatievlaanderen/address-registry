## Create topics
- If not exists create the topics from the scripts

## How to execute per cluster
- put the all script `ALL_01_ADDRESS_SNAPSHOT_OSLO_STREAM` into ksqlDB
- Set auto.offset.reset = earliest
- execute
- now do the same for the specific script(s)

## How to set up the connectors
- put the connectors script into ksqlDB
- replace *** with the secrets from last pass
- Run the script