# 5. Transform the event store to Lambert 2008

Date: 2026-08-28

## Status

Accepted

## Context

[ADR 0003](0003-lambert2008-gml-input-backoffice.md) made the BackOffice API accept Lambert 2008
(EPSG 3812) input while normalizing everything to the event store's reference system.
[ADR 0004](0004-lambert2008-projections-and-oslo.md) made every reader cope with positions in either
reference system. Both left the same two things open: the lambda's `GmlHelpers.ToExtendedWkbGeometry()`,
which still hardcodes Lambert 72, and the transformation of the event store itself.

This ADR covers the transformation: the domain change that expresses it, what each projection does with
it, and the one-shot job that drives it.

## Decision

### One command per stream, one event per address

`TransformToLambert2008` takes a `StreetNamePersistentLocalId` and nothing else — the transformation has
nothing to decide per address, so batching it to the stream keeps the number of commands to the number of
streams (~10^5) rather than the number of addresses (~10^6).

It applies `AddressPositionCrsWasChanged` per address. That name is not an invention: the contract
`Be.Vlaanderen.Basisregisters.GrAr.Contracts.AddressRegistry.AddressPositionCrsWasChanged` already exists
in the version of GrAr.Contracts this repository references, so the Kafka message and its shape were
already settled elsewhere and the domain event mirrors it field for field — including `GeometryMethod` and
`GeometrySpecification`, which the transformation does not change.

Restating those two is the decision that made everything downstream cheap. Every existing
`AddressPositionWasChanged` handler touches only fields the two events share, so all twenty projection
handlers are copies of their `AddressPositionWasChanged` counterpart rather than twenty hand-written
variants. A position-only event was considered and rejected for exactly that reason.

### The aggregate method is deliberately unguarded

`StreetNameAddress.TransformPositionToLambert2008()` has no status guard and no removal guard, unlike
`ChangePosition` and `CorrectPosition`. It is not an edit of the address but a change of the reference
system its position is expressed in, and it has to reach every address the stream holds — removed,
rejected and retired ones included — or the event store would be left holding both systems indefinitely.
`StreetName.TransformToLambert2008()` iterates `StreetNameAddresses` unfiltered for the same reason, and
has no street name status guard: a retired or removed street name holds positions like any other.

An address whose position is already Lambert 2008 applies nothing. That is what makes re-running the
transformation over a stream a no-op rather than a double transform, and it is what the migrator's
restart-heavy operating model depends on.

### `TransformFromLambert72To08`, not `EnsureLambert08`

`EnsureLambert08` only transforms geometries that actually fall inside Flanders and *relabels* everything
else (see ADR 0004). For a projection that is harmless. For the event store it would silently corrupt any
position outside the envelope — writing Lambert 72 coordinates under SRID 3812, ~500 km from where the
address is. The transformation therefore uses `LambertTransformation.TransformFromLambert72To08` with
`roundingPrecision: 2`, which transforms unconditionally, at the centimetre precision positions are
persisted at.

### Reading the current position

`AddressRegistry.WKBReaderFactory.CreateForEwkb` falls back to the Lambert 72 reader for bytes that carry
no SRID, which is what positions written before the event store wrote EWKB look like. Those are Lambert 72
by definition, so they transform like any other and come out carrying SRID 3812 — the transformation
also fixes the missing SRID.

There is a trap worth recording. `Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology` declares a
`WKBReaderFactory` of its own, and a using directive inside `namespace AddressRegistry.StreetName`
outranks `AddressRegistry.WKBReaderFactory` from the enclosing namespace. The unqualified call bound to
GrAr's version, which *throws* on SRID-less EWKB instead of falling back. It compiled, and it passed every
test that used a normal EWKB position. `StreetNameAddress.cs` now aliases the type explicitly.

### What each projection does

The rule is that a reprojection does not change the address. It changes the units its position is
expressed in, so the stored position follows the event store, but nothing that means "this object
changed" reacts.

| Projection | Position | Version |
|---|---|---|
| Legacy detail, AddressMatch detail | updated | **not** bumped |
| Extract | updated | **not** bumped |
| Integration latest item (v1, v2) | updated | **not** bumped |
| WFS V2 / V3, WMS V3 / V4 | updated | **not** bumped |
| Elastic list, search | updated | field omitted from the partial update |
| Feed | document updated | no cloud event, version untouched |
| Legacy syndication | new item, position updated | **not** bumped |
| Integration version | new version row | — |
| LastChangedList (v1, v3) | — | bumped |
| Producers (v3, v4, readdress-fix, Oslo snapshot) | — | produced |

Five of those need their reasoning written down.

**`LastEventHash` is still updated** on the two detail projections, even though the version is not. It is
not a version: Api.Oslo serves it as the ETag and the BackOffice checks the caller's ETag against the
*aggregate's* `LastEventHash`, which the transformation event does change. Freezing the projection's copy
would make the first edit of every address after the transformation fail with a 412.

**Elastic needed a mechanism, not just an omission.** `VersionTimestamp` was a required constructor
argument on `AddressListPartialDocument` / `AddressSearchPartialDocument` and always serialized, so any
partial update overwrote it. It is now `DateTimeOffset?` with `JsonIgnoreCondition.WhenWritingDefault` and
a parameterless constructor; the transformation handler omits the field and Elastic keeps the version it
holds. Every existing call site is unchanged.

**The feed updates the document but produces no cloud event.** Consumers are not told the address changed,
because it did not — but the document has to follow the event store, or the feed would keep serving the
pre-transformation position on every subsequent event. Note that
`context.Entry(document).Property(x => x.Document).IsModified = true` lives inside `AddCloudEvent`: the
`Document` column is not change-tracked, so a handler that skips the cloud event has to mark it itself or
the write is silently dropped.

**LastChangedList is bumped**, unlike the other version-shaped things, because the rendered output really
does change: Api.Oslo version 3 goes from one `geometrie` entry to two once the position it reads is
Lambert 2008 (see ADR 0004), and version 2 can shift by rounding. The caches have to be invalidated.

**Legacy syndication publishes the event but not a version.** It clones a new item as it does for every
other event — so the entry appears in the feed, carrying the transformed `PointPosition` and
`AddressPositionCrsWasChanged` as its `ChangeType` — and then puts `LastChangedOn` back to the value the
address's last real change gave it. Consumers reading the feed sequentially therefore see the
transformation and the new position; consumers keying on the version see no new version, which is the same
rule the rest of the table follows.

**A removed address is not published at all.** Consumers were told it was removed; the transformation is
not something to tell them about, least of all for that address. The syndication item carries no removed
flag, so the projection reads the address's latest entry and treats a removal `ChangeType` as the signal.
That is reliable rather than a guess: every event that can follow a removal either un-removes the address
— `AddressRemovalWasCorrected`, which produces an entry of its own — or is guarded against removed
addresses in the aggregate, and the transformation, the one exception, is what this skips. It is also the
only option that works on the table as it stands: a new column would read false for every address removed
before it was introduced, which is exactly the set that has to be recognised, and the feed is far too
large to rebuild for it.

`AddressBoxNumberSyndicationHelper` is updated even for a removed address. It is not published — it is the
row a box number address's next item is cloned from, including after an `AddressRemovalWasCorrected` — so
leaving it in Lambert 72 would resurrect the old position.

**Every handler has to survive a removed address.** This is the only position event that reaches them —
`ChangePosition` and `CorrectPosition` both guard removal — so a projection's position handlers were
written on the assumption that the address is still there, and three groups of them broke on it:

- **WFS V2 / V3, WMS V3 / V4** keep the row and mark it `Removed`, but `FindAddressDetail*` excludes
  removed addresses by default and *throws* rather than returning null. The handler updated the row
  through `allowUpdateRemovedAddress: true` and then looked the same address up again without it. They now
  pass `allowRemovedAddress: true` and return before the house number label work: a removed address is not
  part of any label, so there is no parent to refresh.
- **Extract** deletes the record on removal, so there is no row at all. Nothing to reproject; the handler
  returns.
- **Elastic list and search** delete the document, and a partial update against a missing document is a
  404. `PartialUpdateDocumentIfExists` treats that as nothing-to-do and still throws on anything else.

The projections that keep a row and flag it — legacy detail, address match, feed, integration latest item
— need nothing, and the append-only ones (integration version, syndication) never had the problem.

**No projection needs a rebuild.** Every one of them handles the event, so each converges on its own as
the transformation runs. That is a property worth keeping rather than a coincidence.

### The migrator

`AddressRegistry.Migrator.Lambert2008` is a console application in the shape of the removed
`AddressRegistry.Migrator.Address`, modernised to match `AddressRegistry.Snapshot.Verifier`. It pages
`[AddressRegistry].[Streams]` filtered to `streetname-%`, loads each aggregate, and dispatches the command.

Its operating model is *stop and evaluate*, not one long run, which drives most of its design:

- **`MaxPagesPerRun`** lets a run do a bounded amount of work and exit cleanly, rather than being killed
  mid-page.
- **`[AddressRegistryLambert2008Migration].[ProcessedStreams]`** records one row per stream with
  `AddressCount`, `ConvertedAddresses`, `LoadMilliseconds` and `DispatchMilliseconds`. Timings are
  persisted rather than only logged so a test run can be *queried* afterwards — cost against address
  count, the slowest streams, percentiles — instead of reconstructed from log lines. Load and dispatch are
  measured separately because they scale with different things (stream length versus addresses converted)
  and which dominates is the thing a test run exists to find out.
- **`IsPageCompleted`** makes the resume cursor a watermark rather than a guess. Streams within a page are
  processed in parallel, so a recorded high id says nothing about the ids below it; a completed page does.
- **The bookkeeping insert is not cancellable.** It records work the event store has already accepted, and
  losing the row on a Ctrl-C would leave a transformed stream looking untransformed.
- **`DryRun` defaults to `true`**, so the job cannot transform by accident. A dry run loads and measures
  every stream and reports how many addresses would be transformed, but dispatch timings are not recorded
  at all rather than recorded as zero.
- **A dry run's bookkeeping is its own.** Rows carry `IsDryRun` and every read filters on it. A dry run
  dispatches nothing, so letting it advance the watermark would make the real run that follows skip every
  stream it measured and transform nothing — the failure mode being an apparently successful run that did
  not happen. Keeping both in one table also keeps both sets of timings queryable, which is what the table
  is for; the primary key is `(Id, IsDryRun)`.

Idempotency at the aggregate covers what the bookkeeping cannot: a stream dispatched but not recorded is
re-dispatched on the next run and applies nothing.

### Deployment

On a test environment, set `DistributedLock:Enabled` to `false`. The lease is five minutes with
`TerminateApplicationOnFailedRenew`, so killing the container mid-page can block the next start until it
expires — which is exactly what the stop-and-evaluate loop does repeatedly.

## Consequences

- The order of the cutover is: freeze editing, run the migrator to completion, then release the lambda's
  `GmlHelpers.ToExtendedWkbGeometry()` fix together with `FeatureToggles:UseLambert2008EventStore` set to
  `true`, then unfreeze. Per ADR 0003 the toggle and the lambda are not independently safe and must land in
  the same step. Under the freeze the relative order of the migrator and the toggle does not actually
  matter — a Lambert 2008 position written into an untransformed stream is skipped by the aggregate, and an
  untransformed stream reached after the toggle flips still transforms — but running the migrator first
  keeps the window in which the two reference systems coexist as short as possible.
- **Every projection holds a mix of reference systems while the transformation runs.** ADR 0004 covers what
  that means per projection; the reason to run this as one pass rather than trickling it is that the mixed
  window is visible, most sharply in Elastic, where geo search moves ~90 m per address as it progresses.
- Kafka consumers receive an `AddressPositionCrsWasChanged` message per address. The feed does not carry
  one, so a consumer reading the feed rather than Kafka sees the new coordinates only on the address's next
  real change.
- The syndication feed carries an entry per transformed address, so a consumer replaying it sees ~10^6
  entries whose only change is the reference system. Their `LastChangedOn` is unchanged, so a consumer
  keying on the version sees nothing new.
- Versions and `VersionTimestamp`s do not move for ~10^6 addresses, so anything downstream that polls "what
  changed since" will not see the transformation. That is the intent, and it is the reason
  LastChangedList is the one exception.
- Re-running the migrator over an already-transformed store is safe and cheap: every stream loads, nothing
  applies, and the bookkeeping table tells you it is done.
