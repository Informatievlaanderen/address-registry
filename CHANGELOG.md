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
