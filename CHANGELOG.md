## [1.18.3](https://github.com/informatievlaanderen/address-registry/compare/v1.18.2...v1.18.3) (2019-12-17)


### Bug Fixes

* sync rename to correct BuildingPersistentLocalIdentifierWasAssigned ([d33480b](https://github.com/informatievlaanderen/address-registry/commit/d33480b))


### Performance Improvements

* sync add seperate index on parcel ([f2a50da](https://github.com/informatievlaanderen/address-registry/commit/f2a50da))

## [1.18.2](https://github.com/informatievlaanderen/address-registry/compare/v1.18.1...v1.18.2) (2019-12-12)


### Bug Fixes

* log to console for syndication ([7c226ff](https://github.com/informatievlaanderen/address-registry/commit/7c226ff))

## [1.18.1](https://github.com/informatievlaanderen/address-registry/compare/v1.18.0...v1.18.1) (2019-12-12)


### Bug Fixes

* remove gemeentid column from extract ([7206a40](https://github.com/informatievlaanderen/address-registry/commit/7206a40))

# [1.18.0](https://github.com/informatievlaanderen/address-registry/compare/v1.17.8...v1.18.0) (2019-12-12)


### Features

* add extract address link projection ([12de6f4](https://github.com/informatievlaanderen/address-registry/commit/12de6f4))

## [1.17.8](https://github.com/informatievlaanderen/address-registry/compare/v1.17.7...v1.17.8) (2019-12-10)


### Performance Improvements

* extract: add indexes on Id and Complete ([e2b1e01](https://github.com/informatievlaanderen/address-registry/commit/e2b1e01))

## [1.17.7](https://github.com/informatievlaanderen/address-registry/compare/v1.17.6...v1.17.7) (2019-12-10)


### Performance Improvements

* increase performance bosa query with only muni filter ([9f5e2ce](https://github.com/informatievlaanderen/address-registry/commit/9f5e2ce))

## [1.17.6](https://github.com/informatievlaanderen/address-registry/compare/v1.17.5...v1.17.6) (2019-12-09)


### Bug Fixes

* bosa search contains now uses Like function ([3379e7e](https://github.com/informatievlaanderen/address-registry/commit/3379e7e))

## [1.17.5](https://github.com/informatievlaanderen/address-registry/compare/v1.17.4...v1.17.5) (2019-12-04)


### Bug Fixes

* streetname filter is now correctly applied on legacy api ([a17aabf](https://github.com/informatievlaanderen/address-registry/commit/a17aabf))

## [1.17.4](https://github.com/informatievlaanderen/address-registry/compare/v1.17.3...v1.17.4) (2019-12-02)


### Bug Fixes

* create new context to filter efficiently in legacy api ([01bfe7d](https://github.com/informatievlaanderen/address-registry/commit/01bfe7d))

## [1.17.3](https://github.com/informatievlaanderen/address-registry/compare/v1.17.2...v1.17.3) (2019-11-29)

## [1.17.2](https://github.com/informatievlaanderen/address-registry/compare/v1.17.1...v1.17.2) (2019-11-28)


### Bug Fixes

* correct postinfo typo detail address in legacy api ([b8be89a](https://github.com/informatievlaanderen/address-registry/commit/b8be89a))

## [1.17.1](https://github.com/informatievlaanderen/address-registry/compare/v1.17.0...v1.17.1) (2019-11-25)

# [1.17.0](https://github.com/informatievlaanderen/address-registry/compare/v1.16.16...v1.17.0) (2019-11-25)


### Features

* add view to count valid addresses efficiently ([fa35a29](https://github.com/informatievlaanderen/address-registry/commit/fa35a29))

## [1.16.16](https://github.com/informatievlaanderen/address-registry/compare/v1.16.15...v1.16.16) (2019-11-18)


### Bug Fixes

* bumped eventhandling dependency ([ca5b395](https://github.com/informatievlaanderen/address-registry/commit/ca5b395))

## [1.16.15](https://github.com/informatievlaanderen/address-registry/compare/v1.16.14...v1.16.15) (2019-10-25)


### Bug Fixes

* filter street/muni name now with or wo diacritics GR-917 ([5ab003c](https://github.com/informatievlaanderen/address-registry/commit/5ab003c))

## [1.16.14](https://github.com/informatievlaanderen/address-registry/compare/v1.16.13...v1.16.14) (2019-10-24)


### Bug Fixes

* use proper gebouweenheidId ([20c48c6](https://github.com/informatievlaanderen/address-registry/commit/20c48c6))

## [1.16.13](https://github.com/informatievlaanderen/address-registry/compare/v1.16.12...v1.16.13) (2019-10-24)

## [1.16.12](https://github.com/informatievlaanderen/address-registry/compare/v1.16.11...v1.16.12) (2019-10-24)


### Bug Fixes

* upgrade grar common ([7bda405](https://github.com/informatievlaanderen/address-registry/commit/7bda405))

## [1.16.11](https://github.com/informatievlaanderen/address-registry/compare/v1.16.10...v1.16.11) (2019-10-22)


### Bug Fixes

* change description for officiallyAssigned GR-929 ([3aa2001](https://github.com/informatievlaanderen/address-registry/commit/3aa2001))

## [1.16.10](https://github.com/informatievlaanderen/address-registry/compare/v1.16.9...v1.16.10) (2019-10-22)


### Bug Fixes

* search streetname ignore diacritics GR-919 ([6327363](https://github.com/informatievlaanderen/address-registry/commit/6327363))

## [1.16.9](https://github.com/informatievlaanderen/address-registry/compare/v1.16.8...v1.16.9) (2019-10-14)


### Bug Fixes

* bosa removed addresses no longer show up ([316315a](https://github.com/informatievlaanderen/address-registry/commit/316315a))

## [1.16.8](https://github.com/informatievlaanderen/address-registry/compare/v1.16.7...v1.16.8) (2019-10-14)


### Performance Improvements

* add index on streetname in address list ([00a3929](https://github.com/informatievlaanderen/address-registry/commit/00a3929))

## [1.16.7](https://github.com/informatievlaanderen/address-registry/compare/v1.16.6...v1.16.7) (2019-10-11)


### Bug Fixes

* push to correct docker repo ([63be6bb](https://github.com/informatievlaanderen/address-registry/commit/63be6bb))
* trigger build ([b103932](https://github.com/informatievlaanderen/address-registry/commit/b103932))
* when no boxnumber present in list then don't show GR-914 ([a241b3b](https://github.com/informatievlaanderen/address-registry/commit/a241b3b))

## [1.16.6](https://github.com/informatievlaanderen/address-registry/compare/v1.16.5...v1.16.6) (2019-10-09)


### Bug Fixes

* add homonym addition if present GR-912 ([e466e95](https://github.com/informatievlaanderen/address-registry/commit/e466e95))
* correct address example ([9c2345b](https://github.com/informatievlaanderen/address-registry/commit/9c2345b))
* if no homonymaddition don't show in json instead of null GR-909 ([83fe469](https://github.com/informatievlaanderen/address-registry/commit/83fe469))
* when no boxnumber then don't show in json instead of null GR-910 ([f777841](https://github.com/informatievlaanderen/address-registry/commit/f777841))

## [1.16.5](https://github.com/informatievlaanderen/address-registry/compare/v1.16.4...v1.16.5) (2019-10-01)


### Bug Fixes

* crab subaddress now returns boxnumber in complete address GR-907 ([0340dc7](https://github.com/informatievlaanderen/address-registry/commit/0340dc7))

## [1.16.4](https://github.com/informatievlaanderen/address-registry/compare/v1.16.3...v1.16.4) (2019-09-30)


### Bug Fixes

* check removed before completeness GR-900 ([097ab4c](https://github.com/informatievlaanderen/address-registry/commit/097ab4c))

## [1.16.3](https://github.com/informatievlaanderen/address-registry/compare/v1.16.2...v1.16.3) (2019-09-30)


### Bug Fixes

* added binaries and config bindings for importers ([8975628](https://github.com/informatievlaanderen/address-registry/commit/8975628))

## [1.16.2](https://github.com/informatievlaanderen/address-registry/compare/v1.16.1...v1.16.2) (2019-09-26)


### Bug Fixes

* correct adres list item xml name GR-902 ([ff25c20](https://github.com/informatievlaanderen/address-registry/commit/ff25c20))

## [1.16.1](https://github.com/informatievlaanderen/address-registry/compare/v1.16.0...v1.16.1) (2019-09-26)


### Bug Fixes

* update asset to fix importer ([55ac69c](https://github.com/informatievlaanderen/address-registry/commit/55ac69c))

# [1.16.0](https://github.com/informatievlaanderen/address-registry/compare/v1.15.7...v1.16.0) (2019-09-26)


### Bug Fixes

* fix using ([a5b10b4](https://github.com/informatievlaanderen/address-registry/commit/a5b10b4))


### Features

* upgrade projector and resume projections on startup ([581b07a](https://github.com/informatievlaanderen/address-registry/commit/581b07a))

## [1.15.7](https://github.com/informatievlaanderen/address-registry/compare/v1.15.6...v1.15.7) (2019-09-23)


### Bug Fixes

* correct bosa xml names to be consistent with prd GR-893 ([ca4a96a](https://github.com/informatievlaanderen/address-registry/commit/ca4a96a))
* fix crash when no filter was applied GR-890 ([572f753](https://github.com/informatievlaanderen/address-registry/commit/572f753))

## [1.15.6](https://github.com/informatievlaanderen/address-registry/compare/v1.15.5...v1.15.6) (2019-09-23)


### Bug Fixes

* tweak logging ([a614b9d](https://github.com/informatievlaanderen/address-registry/commit/a614b9d))

## [1.15.5](https://github.com/informatievlaanderen/address-registry/compare/v1.15.4...v1.15.5) (2019-09-23)


### Bug Fixes

* returns empty list when query parameter is incorrect GR-889 ([602186d](https://github.com/informatievlaanderen/address-registry/commit/602186d))

## [1.15.4](https://github.com/informatievlaanderen/address-registry/compare/v1.15.3...v1.15.4) (2019-09-23)


### Bug Fixes

* create new context for complex bosa queries GR-870 ([87dd7da](https://github.com/informatievlaanderen/address-registry/commit/87dd7da))

## [1.15.3](https://github.com/informatievlaanderen/address-registry/compare/v1.15.2...v1.15.3) (2019-09-20)


### Bug Fixes

* hide busnummer from housenumber endpoint GR-887 ([356f591](https://github.com/informatievlaanderen/address-registry/commit/356f591))

## [1.15.2](https://github.com/informatievlaanderen/address-registry/compare/v1.15.1...v1.15.2) (2019-09-20)


### Bug Fixes

* CRAB endpoints return deleted addresses GR-886 ([948292a](https://github.com/informatievlaanderen/address-registry/commit/948292a))

## [1.15.1](https://github.com/informatievlaanderen/address-registry/compare/v1.15.0...v1.15.1) (2019-09-19)


### Bug Fixes

* clean up EF NTS refs which causes crash ([7923c55](https://github.com/informatievlaanderen/address-registry/commit/7923c55))
* remove old nts types ([4257b17](https://github.com/informatievlaanderen/address-registry/commit/4257b17))
* update dependencies for build ([3662cc4](https://github.com/informatievlaanderen/address-registry/commit/3662cc4))

# [1.15.0](https://github.com/informatievlaanderen/address-registry/compare/v1.14.6...v1.15.0) (2019-09-19)


### Features

* upgrade NTS and Shaperon packages ([74115c3](https://github.com/informatievlaanderen/address-registry/commit/74115c3))

## [1.14.6](https://github.com/informatievlaanderen/address-registry/compare/v1.14.5...v1.14.6) (2019-09-18)


### Bug Fixes

* add missing connection string ([e1d99ee](https://github.com/informatievlaanderen/address-registry/commit/e1d99ee))

## [1.14.5](https://github.com/informatievlaanderen/address-registry/compare/v1.14.4...v1.14.5) (2019-09-17)


### Bug Fixes

* add sqlclient to addressmatch import ([058e59e](https://github.com/informatievlaanderen/address-registry/commit/058e59e))
* use generic dbtraceconnection ([44954d4](https://github.com/informatievlaanderen/address-registry/commit/44954d4))

## [1.14.4](https://github.com/informatievlaanderen/address-registry/compare/v1.14.3...v1.14.4) (2019-09-17)


### Bug Fixes

* fix contains search ([f7c70f3](https://github.com/informatievlaanderen/address-registry/commit/f7c70f3))

## [1.14.3](https://github.com/informatievlaanderen/address-registry/compare/v1.14.2...v1.14.3) (2019-09-16)


### Bug Fixes

* bosa null busnummer is now hidden GR-867 ([c9775d2](https://github.com/informatievlaanderen/address-registry/commit/c9775d2))
* postinfo version bosa now uses syndication table GR-868 ([1a89a2b](https://github.com/informatievlaanderen/address-registry/commit/1a89a2b))

## [1.14.2](https://github.com/informatievlaanderen/address-registry/compare/v1.14.1...v1.14.2) (2019-09-16)


### Bug Fixes

* bosa default searchtype muni & street is now contains GR-869 ([311afb6](https://github.com/informatievlaanderen/address-registry/commit/311afb6))

## [1.14.1](https://github.com/informatievlaanderen/address-registry/compare/v1.14.0...v1.14.1) (2019-09-16)


### Performance Improvements

* bosa get only relevant data when ToList streetnames ([bbc925f](https://github.com/informatievlaanderen/address-registry/commit/bbc925f))

# [1.14.0](https://github.com/informatievlaanderen/address-registry/compare/v1.13.4...v1.14.0) (2019-09-16)


### Features

* add version indexes for bosa ([d061b1b](https://github.com/informatievlaanderen/address-registry/commit/d061b1b))

## [1.13.4](https://github.com/informatievlaanderen/address-registry/compare/v1.13.3...v1.13.4) (2019-09-16)


### Bug Fixes

* bosa exactsearch on street & muni GR-871 ([5bcf4f6](https://github.com/informatievlaanderen/address-registry/commit/5bcf4f6))
* empty collection instead of null GR-865 ([458c809](https://github.com/informatievlaanderen/address-registry/commit/458c809))

## [1.13.3](https://github.com/informatievlaanderen/address-registry/compare/v1.13.2...v1.13.3) (2019-09-13)


### Bug Fixes

* add municipality bosa syndication index ([c7dc77a](https://github.com/informatievlaanderen/address-registry/commit/c7dc77a))

## [1.13.2](https://github.com/informatievlaanderen/address-registry/compare/v1.13.1...v1.13.2) (2019-09-13)


### Bug Fixes

* tune bosa address query for performance ([9fa52db](https://github.com/informatievlaanderen/address-registry/commit/9fa52db))

## [1.13.1](https://github.com/informatievlaanderen/address-registry/compare/v1.13.0...v1.13.1) (2019-09-13)


### Bug Fixes

* update redis lastchangedlist to log time of lasterror ([cdc2a39](https://github.com/informatievlaanderen/address-registry/commit/cdc2a39))

# [1.13.0](https://github.com/informatievlaanderen/address-registry/compare/v1.12.15...v1.13.0) (2019-09-12)


### Features

* keep track of how many times lastchanged has errored ([1991955](https://github.com/informatievlaanderen/address-registry/commit/1991955))

## [1.12.15](https://github.com/informatievlaanderen/address-registry/compare/v1.12.14...v1.12.15) (2019-09-06)


### Bug Fixes

* add tracing to legacycontext ([9a12b3c](https://github.com/informatievlaanderen/address-registry/commit/9a12b3c))
* use correct extractmodule in api ([1c6c98e](https://github.com/informatievlaanderen/address-registry/commit/1c6c98e))

## [1.12.14](https://github.com/informatievlaanderen/address-registry/compare/v1.12.13...v1.12.14) (2019-09-05)


### Bug Fixes

* initial jira version ([0c75478](https://github.com/informatievlaanderen/address-registry/commit/0c75478))

## [1.12.13](https://github.com/informatievlaanderen/address-registry/compare/v1.12.12...v1.12.13) (2019-09-03)


### Bug Fixes

* update problemdetails for xml response GR-829 ([f615a5b](https://github.com/informatievlaanderen/address-registry/commit/f615a5b))

## [1.12.12](https://github.com/informatievlaanderen/address-registry/compare/v1.12.11...v1.12.12) (2019-09-03)


### Bug Fixes

* update problemdetails for xml response GR-829 ([aa28855](https://github.com/informatievlaanderen/address-registry/commit/aa28855))

## [1.12.11](https://github.com/informatievlaanderen/address-registry/compare/v1.12.10...v1.12.11) (2019-09-03)


### Bug Fixes

* syndication muni in wrong schema ([261e4f2](https://github.com/informatievlaanderen/address-registry/commit/261e4f2))

## [1.12.10](https://github.com/informatievlaanderen/address-registry/compare/v1.12.9...v1.12.10) (2019-09-03)


### Bug Fixes

* syndications use wrong parcel and buildingUnit names ([ac6a2c7](https://github.com/informatievlaanderen/address-registry/commit/ac6a2c7))

## [1.12.9](https://github.com/informatievlaanderen/address-registry/compare/v1.12.8...v1.12.9) (2019-09-02)


### Bug Fixes

* do not log to console write ([cb851db](https://github.com/informatievlaanderen/address-registry/commit/cb851db))

## [1.12.8](https://github.com/informatievlaanderen/address-registry/compare/v1.12.7...v1.12.8) (2019-09-02)


### Bug Fixes

* properly report errors ([9d56cc6](https://github.com/informatievlaanderen/address-registry/commit/9d56cc6))

## [1.12.7](https://github.com/informatievlaanderen/address-registry/compare/v1.12.6...v1.12.7) (2019-09-02)


### Bug Fixes

* cleanup projections and solutioninfo ([6582b92](https://github.com/informatievlaanderen/address-registry/commit/6582b92))

## [1.12.6](https://github.com/informatievlaanderen/address-registry/compare/v1.12.5...v1.12.6) (2019-09-02)


### Bug Fixes

* correct bosa syndication schema ([86c20ba](https://github.com/informatievlaanderen/address-registry/commit/86c20ba))

## [1.12.5](https://github.com/informatievlaanderen/address-registry/compare/v1.12.4...v1.12.5) (2019-08-29)


### Bug Fixes

* use columnstore for legacy syndication ([9f6cea1](https://github.com/informatievlaanderen/address-registry/commit/9f6cea1))

## [1.12.4](https://github.com/informatievlaanderen/address-registry/compare/v1.12.3...v1.12.4) (2019-08-27)


### Bug Fixes

* make datadog tracing check more for nulls ([dec2e9c](https://github.com/informatievlaanderen/address-registry/commit/dec2e9c))

## [1.12.3](https://github.com/informatievlaanderen/address-registry/compare/v1.12.2...v1.12.3) (2019-08-27)


### Bug Fixes

* use new desiredstate columns for projections ([8fde1c0](https://github.com/informatievlaanderen/address-registry/commit/8fde1c0))

## [1.12.2](https://github.com/informatievlaanderen/address-registry/compare/v1.12.1...v1.12.2) (2019-08-26)


### Bug Fixes

* use fixed datadog tracing ([ae52bd5](https://github.com/informatievlaanderen/address-registry/commit/ae52bd5))

## [1.12.1](https://github.com/informatievlaanderen/address-registry/compare/v1.12.0...v1.12.1) (2019-08-26)


### Bug Fixes

* fix swagger ([6d6c258](https://github.com/informatievlaanderen/address-registry/commit/6d6c258))

# [1.12.0](https://github.com/informatievlaanderen/address-registry/compare/v1.11.0...v1.12.0) (2019-08-26)


### Features

* bump to .net 2.2.6 ([591f32d](https://github.com/informatievlaanderen/address-registry/commit/591f32d))

# [1.11.0](https://github.com/informatievlaanderen/address-registry/compare/v1.10.3...v1.11.0) (2019-08-22)


### Features

* extract datavlaanderen namespace to settings ([efb4ebc](https://github.com/informatievlaanderen/address-registry/commit/efb4ebc))

## [1.10.3](https://github.com/informatievlaanderen/address-registry/compare/v1.10.2...v1.10.3) (2019-08-22)


### Bug Fixes

* bosa exact filter takes exact name into account ([b62d607](https://github.com/informatievlaanderen/address-registry/commit/b62d607))
* return empty response when request has invalid data GR-856 ([035a6ae](https://github.com/informatievlaanderen/address-registry/commit/035a6ae))

## [1.10.2](https://github.com/informatievlaanderen/address-registry/compare/v1.10.1...v1.10.2) (2019-08-20)


### Bug Fixes

* detail url for crab endpoints are now correctly formatted ([b1e4441](https://github.com/informatievlaanderen/address-registry/commit/b1e4441))

## [1.10.1](https://github.com/informatievlaanderen/address-registry/compare/v1.10.0...v1.10.1) (2019-08-19)


### Bug Fixes

* filter null value on crab endpoints now acts accordingly ([ed98e58](https://github.com/informatievlaanderen/address-registry/commit/ed98e58))

# [1.10.0](https://github.com/informatievlaanderen/address-registry/compare/v1.9.1...v1.10.0) (2019-08-19)


### Features

* add wait for user input to importers ([fd88bcc](https://github.com/informatievlaanderen/address-registry/commit/fd88bcc))

## [1.9.1](https://github.com/informatievlaanderen/address-registry/compare/v1.9.0...v1.9.1) (2019-08-14)


### Bug Fixes

* remove Unicode control characters in domain for Box/HouseNumber ([249690e](https://github.com/informatievlaanderen/address-registry/commit/249690e))

# [1.9.0](https://github.com/informatievlaanderen/address-registry/compare/v1.8.0...v1.9.0) (2019-08-14)


### Features

* add missing sync building handlers when nothing was expected [#23](https://github.com/informatievlaanderen/address-registry/issues/23) ([b513c30](https://github.com/informatievlaanderen/address-registry/commit/b513c30))

# [1.8.0](https://github.com/informatievlaanderen/address-registry/compare/v1.7.1...v1.8.0) (2019-08-13)


### Features

* add missing event handlers where nothing was expected [#23](https://github.com/informatievlaanderen/address-registry/issues/23) ([0970d3e](https://github.com/informatievlaanderen/address-registry/commit/0970d3e))

## [1.7.1](https://github.com/informatievlaanderen/address-registry/compare/v1.7.0...v1.7.1) (2019-08-09)

# [1.7.0](https://github.com/informatievlaanderen/address-registry/compare/v1.6.3...v1.7.0) (2019-08-06)


### Features

* change Point to ByteArray in legacy to prevent memory leak ([73286d4](https://github.com/informatievlaanderen/address-registry/commit/73286d4))

## [1.6.3](https://github.com/informatievlaanderen/address-registry/compare/v1.6.2...v1.6.3) (2019-07-17)


### Bug Fixes

* do not hardcode logging to console ([32d5417](https://github.com/informatievlaanderen/address-registry/commit/32d5417))
* do not hardcode logging to console ([7ee8776](https://github.com/informatievlaanderen/address-registry/commit/7ee8776))

## [1.6.2](https://github.com/informatievlaanderen/address-registry/compare/v1.6.1...v1.6.2) (2019-07-15)


### Bug Fixes

* remove oslo references for importers ([fcb0c02](https://github.com/informatievlaanderen/address-registry/commit/fcb0c02))

## [1.6.1](https://github.com/informatievlaanderen/address-registry/compare/v1.6.0...v1.6.1) (2019-07-15)


### Bug Fixes

* allow parallel generation of oslo id ([54d5222](https://github.com/informatievlaanderen/address-registry/commit/54d5222))
* correct timestamp on deletes with timestamp before create ([8a2a044](https://github.com/informatievlaanderen/address-registry/commit/8a2a044))
* exclude subaddresses with no housenubers ([30ec38a](https://github.com/informatievlaanderen/address-registry/commit/30ec38a))
* remove slow paging from housnumberid query ([f8c92f6](https://github.com/informatievlaanderen/address-registry/commit/f8c92f6))

# [1.6.0](https://github.com/informatievlaanderen/address-registry/compare/v1.5.2...v1.6.0) (2019-07-11)


### Features

* rename OsloId to PersistentLocalId ([9092ca2](https://github.com/informatievlaanderen/address-registry/commit/9092ca2))

## [1.5.2](https://github.com/informatievlaanderen/address-registry/compare/v1.5.1...v1.5.2) (2019-07-08)


### Bug Fixes

* use correct docker ids ([121c1dd](https://github.com/informatievlaanderen/address-registry/commit/121c1dd))
* use correct projector project ([6815f7b](https://github.com/informatievlaanderen/address-registry/commit/6815f7b))
* use syndication docker image ([11c8ac1](https://github.com/informatievlaanderen/address-registry/commit/11c8ac1))

## [1.5.1](https://github.com/informatievlaanderen/address-registry/compare/v1.5.0...v1.5.1) (2019-07-08)


### Bug Fixes

* include version number to deploy ([83b7db8](https://github.com/informatievlaanderen/address-registry/commit/83b7db8))

# [1.5.0](https://github.com/informatievlaanderen/address-registry/compare/v1.4.0...v1.5.0) (2019-07-08)


### Bug Fixes

* include init.sh files ([f43f2b9](https://github.com/informatievlaanderen/address-registry/commit/f43f2b9))


### Features

* prepare for deploy ([30fa25c](https://github.com/informatievlaanderen/address-registry/commit/30fa25c))

# [1.4.0](https://github.com/informatievlaanderen/address-registry/compare/v1.3.1...v1.4.0) (2019-07-08)


### Features

* add examples for crab api legacy endpoints ([6966d18](https://github.com/informatievlaanderen/address-registry/commit/6966d18))

## [1.3.1](https://github.com/informatievlaanderen/address-registry/compare/v1.3.0...v1.3.1) (2019-07-04)


### Bug Fixes

* use corrext next uris for crabhousenumber and crabsubaddress ([100a080](https://github.com/informatievlaanderen/address-registry/commit/100a080))

# [1.3.0](https://github.com/informatievlaanderen/address-registry/compare/v1.2.0...v1.3.0) (2019-06-24)


### Bug Fixes

* add addresscomparer to compare Point ([6ec31b2](https://github.com/informatievlaanderen/address-registry/commit/6ec31b2))
* correct sync Point to SyndicationPoint ([58b814c](https://github.com/informatievlaanderen/address-registry/commit/58b814c))
* remove Lifetime.EndDate from identifier fields ([ed66317](https://github.com/informatievlaanderen/address-registry/commit/ed66317))
* upgrade batch configuration in importers ([7f7eadc](https://github.com/informatievlaanderen/address-registry/commit/7f7eadc))
* write xml via xml writer for event data ([a975c0d](https://github.com/informatievlaanderen/address-registry/commit/a975c0d))


### Features

* change db type geometry + complete syndication ([8019b60](https://github.com/informatievlaanderen/address-registry/commit/8019b60))
* get import status from import api ([abf31c7](https://github.com/informatievlaanderen/address-registry/commit/abf31c7))
* upgrade grar packages ([59b5a61](https://github.com/informatievlaanderen/address-registry/commit/59b5a61))
* upgrade packages grar to 10.0 ([12a69c2](https://github.com/informatievlaanderen/address-registry/commit/12a69c2))
* upgrade projectionhandling and fix logging of output in tests ([8b007ea](https://github.com/informatievlaanderen/address-registry/commit/8b007ea))

# [1.2.0](https://github.com/informatievlaanderen/address-registry/compare/v1.1.3...v1.2.0) (2019-06-12)


### Bug Fixes

* correct event name to MunicipalityWasAttached ([be7a40d](https://github.com/informatievlaanderen/address-registry/commit/be7a40d))
* correct parcel event typo ([c2133e4](https://github.com/informatievlaanderen/address-registry/commit/c2133e4))
* correct syndication example to build ([bc50e72](https://github.com/informatievlaanderen/address-registry/commit/bc50e72))
* fix tests addressmatch after refactor ([c981329](https://github.com/informatievlaanderen/address-registry/commit/c981329))
* remove deleted files from csproj ([d17a01b](https://github.com/informatievlaanderen/address-registry/commit/d17a01b))
* set oslo id to integer for extract ([a4e9d40](https://github.com/informatievlaanderen/address-registry/commit/a4e9d40))


### Features

* add addressmatch import in api into tmp tables ([da31094](https://github.com/informatievlaanderen/address-registry/commit/da31094))
* add buildingunits syndication to addressmatch ([2c76173](https://github.com/informatievlaanderen/address-registry/commit/2c76173))
* add eventdata to sync ([b0cd328](https://github.com/informatievlaanderen/address-registry/commit/b0cd328))
* add Importer to create and zip files ([982a8ad](https://github.com/informatievlaanderen/address-registry/commit/982a8ad))
* add parcel syndication for addressmatch ([b2a8131](https://github.com/informatievlaanderen/address-registry/commit/b2a8131))
* add parcel to addressmatch response ([50b1240](https://github.com/informatievlaanderen/address-registry/commit/50b1240))
* add postal info syndication ([54f27a4](https://github.com/informatievlaanderen/address-registry/commit/54f27a4))
* add response examples addressmatch ([9ccf351](https://github.com/informatievlaanderen/address-registry/commit/9ccf351))
* add testing infrastructure legacy addressmatch + 1 test working ([6d8b162](https://github.com/informatievlaanderen/address-registry/commit/6d8b162))
* completed import + upload to crab tables ([f670336](https://github.com/informatievlaanderen/address-registry/commit/f670336))
* ported addressmatch tests + some clean up ([696427b](https://github.com/informatievlaanderen/address-registry/commit/696427b))
* ported api for addressmatch (WIP) ([977bb5f](https://github.com/informatievlaanderen/address-registry/commit/977bb5f))
* ported old addressmatch code ([b79ca94](https://github.com/informatievlaanderen/address-registry/commit/b79ca94))
* upgrade to netcore 2.2.4 + validations ([22a9b7b](https://github.com/informatievlaanderen/address-registry/commit/22a9b7b))

## [1.1.3](https://github.com/informatievlaanderen/address-registry/compare/v1.1.2...v1.1.3) (2019-05-21)

## [1.1.2](https://github.com/informatievlaanderen/address-registry/compare/v1.1.1...v1.1.2) (2019-05-20)

## [1.1.1](https://github.com/informatievlaanderen/address-registry/compare/v1.1.0...v1.1.1) (2019-05-17)


### Bug Fixes

* remove test code from applications ([456784c](https://github.com/informatievlaanderen/address-registry/commit/456784c))

# [1.1.0](https://github.com/informatievlaanderen/address-registry/compare/v1.0.8...v1.1.0) (2019-04-30)


### Features

* add projector + clean up projection libs ([32887dd](https://github.com/informatievlaanderen/address-registry/commit/32887dd))
* moved bosa sync from legacy projections to syndication ([a6cd8be](https://github.com/informatievlaanderen/address-registry/commit/a6cd8be))
* upgrade packages ([bcf33bc](https://github.com/informatievlaanderen/address-registry/commit/bcf33bc))

## [1.0.8](https://github.com/informatievlaanderen/address-registry/compare/v1.0.7...v1.0.8) (2019-04-18)

## [1.0.7](https://github.com/informatievlaanderen/address-registry/compare/v1.0.6...v1.0.7) (2019-04-17)


### Bug Fixes

* volgende is now not emitted if null ([bc4e1e6](https://github.com/informatievlaanderen/address-registry/commit/bc4e1e6))

## [1.0.6](https://github.com/informatievlaanderen/address-registry/compare/v1.0.5...v1.0.6) (2019-04-16)


### Bug Fixes

* sort address list on olsoid (objectid) by default [GR-717] ([69bdc2f](https://github.com/informatievlaanderen/address-registry/commit/69bdc2f))

## [1.0.5](https://github.com/informatievlaanderen/address-registry/compare/v1.0.4...v1.0.5) (2019-03-11)

## [1.0.4](https://github.com/informatievlaanderen/address-registry/compare/v1.0.3...v1.0.4) (2019-03-08)


### Bug Fixes

* get rid of testing connectingstring (password is changed!) ([83a4c9f](https://github.com/informatievlaanderen/address-registry/commit/83a4c9f))

## [1.0.3](https://github.com/informatievlaanderen/address-registry/compare/v1.0.2...v1.0.3) (2019-03-05)


### Bug Fixes

* correct StreetName event typo ([9a08f8e](https://github.com/informatievlaanderen/address-registry/commit/9a08f8e))

## [1.0.2](https://github.com/informatievlaanderen/address-registry/compare/v1.0.1...v1.0.2) (2019-03-05)


### Bug Fixes

* correct typo municipality event ([d62492a](https://github.com/informatievlaanderen/address-registry/commit/d62492a))

## [1.0.1](https://github.com/informatievlaanderen/address-registry/compare/v1.0.0...v1.0.1) (2019-03-05)


### Bug Fixes

* correct typo syndication municipality event ([6b5c615](https://github.com/informatievlaanderen/address-registry/commit/6b5c615))

# 1.0.0 (2019-03-04)


### Features

* open source with EUPL-1.2 license as 'agentschap Informatie Vlaanderen' ([bd804e3](https://github.com/informatievlaanderen/address-registry/commit/bd804e3))
