# 2. Separate Address Match

Date: 2024-10-04

## Status

Accepted

## Context

AddressMatch takes a big load on our projections database and can cause other endpoints to slow down or error.

## Decision

We will split address match up in a separate projection and database.

## Consequences

Will increase infrastructure costs but the `address-match` database can be scaled separately.
Extra projection => extra maintenance.
