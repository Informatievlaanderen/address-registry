# [4.13.0](https://github.com/informatievlaanderen/address-registry/compare/v4.12.4...v4.13.0) (2024-08-20)


### Features

* **consumer:** add offset as projection state to read consumers ([d25db8c](https://github.com/informatievlaanderen/address-registry/commit/d25db8c45c6ba8773dbfdfb94540d12db350ee51))

## [4.12.4](https://github.com/informatievlaanderen/address-registry/compare/v4.12.3...v4.12.4) (2024-08-19)


### Bug Fixes

* **elastic:** multidoc when empty should return empty ([6e705a5](https://github.com/informatievlaanderen/address-registry/commit/6e705a52b621d1af1fc05b76a19168abc0c353ce))

## [4.12.3](https://github.com/informatievlaanderen/address-registry/compare/v4.12.2...v4.12.3) (2024-08-12)


### Bug Fixes

* add option to enable elastic debug information ([#1211](https://github.com/informatievlaanderen/address-registry/issues/1211)) ([e6ffbc0](https://github.com/informatievlaanderen/address-registry/commit/e6ffbc0377636003439433b1046f74e1ea98559a))

## [4.12.2](https://github.com/informatievlaanderen/address-registry/compare/v4.12.1...v4.12.2) (2024-08-12)


### Bug Fixes

* add elastic ServerError to custom exception ([#1210](https://github.com/informatievlaanderen/address-registry/issues/1210)) ([40ed814](https://github.com/informatievlaanderen/address-registry/commit/40ed814e555661589022f8646bd49d1b3b59cdfd))

## [4.12.1](https://github.com/informatievlaanderen/address-registry/compare/v4.12.0...v4.12.1) (2024-08-12)


### Bug Fixes

* version bump ([77014c3](https://github.com/informatievlaanderen/address-registry/commit/77014c373d2511f73fec7fc3d5e44249b7e7a24e))

# [4.12.0](https://github.com/informatievlaanderen/address-registry/compare/v4.11.3...v4.12.0) (2024-08-09)


### Features

* elastic projections ([#1209](https://github.com/informatievlaanderen/address-registry/issues/1209)) ([7cdb160](https://github.com/informatievlaanderen/address-registry/commit/7cdb1606dcdee70b880db8081c99475cb9fafc1f))

## [4.11.3](https://github.com/informatievlaanderen/address-registry/compare/v4.11.2...v4.11.3) (2024-07-30)


### Bug Fixes

* remove v1 mgiration ([59bc94c](https://github.com/informatievlaanderen/address-registry/commit/59bc94cac4c289f061a03d2570995258c4c91631))

## [4.11.2](https://github.com/informatievlaanderen/address-registry/compare/v4.11.1...v4.11.2) (2024-07-30)


### Bug Fixes

* rejected or retired address because of municipality merger doesn't always have a new address ([aca2365](https://github.com/informatievlaanderen/address-registry/commit/aca2365e8dd1973f44045d18a33afcf6997b7334))

## [4.11.1](https://github.com/informatievlaanderen/address-registry/compare/v4.11.0...v4.11.1) (2024-07-30)

# [4.11.0](https://github.com/informatievlaanderen/address-registry/compare/v4.10.0...v4.11.0) (2024-07-29)


### Bug Fixes

* add streetnamenameswerechanged ([e47420e](https://github.com/informatievlaanderen/address-registry/commit/e47420ec383df3d9be2fbf38cacc010e4296d088))


### Features

* add snapshot status endpoint ([#1205](https://github.com/informatievlaanderen/address-registry/issues/1205)) ([8710899](https://github.com/informatievlaanderen/address-registry/commit/87108996be0a270542e6cbffc70932e55bcea94e))
* add streetname municipality merger events to read streetname consumer ([d7cb54a](https://github.com/informatievlaanderen/address-registry/commit/d7cb54ac448e2059d538763f43c92abd992b93df))
* blacklist ovocodes ([#1204](https://github.com/informatievlaanderen/address-registry/issues/1204)) ([ccffc0b](https://github.com/informatievlaanderen/address-registry/commit/ccffc0bc13065afeab8fe7faba3fbae1d775bfac))
* consume MunicipalityWasMerged ([e433ead](https://github.com/informatievlaanderen/address-registry/commit/e433ead1c4a197d56bac0839a4c743365e6e4ebc))
* consume streetname municipality merger events ([e3fdf1d](https://github.com/informatievlaanderen/address-registry/commit/e3fdf1d71233c2bacfef3718cb3af058e3cf8f1d))

# [4.10.0](https://github.com/informatievlaanderen/address-registry/compare/v4.9.0...v4.10.0) (2024-07-18)


### Bug Fixes

* syndication querycontext ([458fbd0](https://github.com/informatievlaanderen/address-registry/commit/458fbd062df7c0781a49953e7274630733e71aa0))


### Features

* remove syndication project ([2b6d455](https://github.com/informatievlaanderen/address-registry/commit/2b6d45500299ddce84058cff83901555525338a7))

# [4.9.0](https://github.com/informatievlaanderen/address-registry/compare/v4.8.1...v4.9.0) (2024-07-18)


### Features

* propose address for municipality merger ([e8f77e6](https://github.com/informatievlaanderen/address-registry/commit/e8f77e6d060d3338a503dd4ada2d95f529d961e3))
* remove api.legacy + remove syndicationcontext dep ([ff56904](https://github.com/informatievlaanderen/address-registry/commit/ff5690464915e67ae3d7fae20fd57ac558609773))

## [4.8.1](https://github.com/informatievlaanderen/address-registry/compare/v4.8.0...v4.8.1) (2024-07-17)


### Bug Fixes

* add migrations for consumer postal ([06d21cc](https://github.com/informatievlaanderen/address-registry/commit/06d21cc85307f9a68f94eeceaea8140bb528c1bb))

# [4.8.0](https://github.com/informatievlaanderen/address-registry/compare/v4.7.0...v4.8.0) (2024-07-17)


### Features

* add postal consumer ([15c2b94](https://github.com/informatievlaanderen/address-registry/commit/15c2b94197a73ceb29852e64ce35df2d30391c2c))

# [4.7.0](https://github.com/informatievlaanderen/address-registry/compare/v4.6.2...v4.7.0) (2024-07-02)


### Features

* add syndication to oslo api ([a5ebf72](https://github.com/informatievlaanderen/address-registry/commit/a5ebf726a314e8c9b15f1db41a76e0ec386b3e59))

## [4.6.2](https://github.com/informatievlaanderen/address-registry/compare/v4.6.1...v4.6.2) (2024-06-19)


### Bug Fixes

* **ci:** change deploy lambda tst+stg ([1f1be96](https://github.com/informatievlaanderen/address-registry/commit/1f1be96d9fda35d799cf9b687b108a4bd1140dc1))

## [4.6.1](https://github.com/informatievlaanderen/address-registry/compare/v4.6.0...v4.6.1) (2024-06-18)


### Bug Fixes

* style to trigger bump ([8c94498](https://github.com/informatievlaanderen/address-registry/commit/8c94498e53a55a2f030995353b59198b9358a03e))

# [4.6.0](https://github.com/informatievlaanderen/address-registry/compare/v4.5.1...v4.6.0) (2024-06-06)


### Features

* new address fix readdress topic ([ea9ff97](https://github.com/informatievlaanderen/address-registry/commit/ea9ff97b732a8d0bed308f63e354666f58519fe3))

## [4.5.1](https://github.com/informatievlaanderen/address-registry/compare/v4.5.0...v4.5.1) (2024-06-03)


### Bug Fixes

* keep event provenance timestamp for new commands ([01ef222](https://github.com/informatievlaanderen/address-registry/commit/01ef222ed6f1e8d9dd062b523605e662bfaf1baa))
* no longer cache v1 endpoints ([75a12b8](https://github.com/informatievlaanderen/address-registry/commit/75a12b8114dbf138b6ba52cbce320402b5ed7b57))
* provenance timestamp in 2nd streetname consumer ([107b3ca](https://github.com/informatievlaanderen/address-registry/commit/107b3cae1a0bcf0836cfcb494e634ecccb524b36))

# [4.5.0](https://github.com/informatievlaanderen/address-registry/compare/v4.4.1...v4.5.0) (2024-05-17)


### Bug Fixes

* add delay to backoffice projections to avoid conflict with lambda handler ([1575b6e](https://github.com/informatievlaanderen/address-registry/commit/1575b6e4a59998da284f9240dd0748e5a13c4e7c))


### Features

* add new StreetnameWasReaddressed ([1b69ab3](https://github.com/informatievlaanderen/address-registry/commit/1b69ab3da2a36c9a375a41f873a3134d4e89a193))

## [4.4.1](https://github.com/informatievlaanderen/address-registry/compare/v4.4.0...v4.4.1) (2024-05-17)


### Bug Fixes

* **ci:** add newstg pipeline + add version to prd ([83cb6cb](https://github.com/informatievlaanderen/address-registry/commit/83cb6cb6a7c77a9b7c6841a00a3324d18318e148))

# [4.4.0](https://github.com/informatievlaanderen/address-registry/compare/v4.3.0...v4.4.0) (2024-05-10)


### Bug Fixes

* AddressRemovalWasCorrected should contain all address properties ([14cad3c](https://github.com/informatievlaanderen/address-registry/commit/14cad3c9cfffe64b39f41c12f59169246a2a5252))
* use HouseNumberAddress status when invalid ([03fb7e3](https://github.com/informatievlaanderen/address-registry/commit/03fb7e3b117d227733ca9c834928a328b5ce72a0))


### Features

* add AddressRemovalWasCorrected projections + producers ([a609322](https://github.com/informatievlaanderen/address-registry/commit/a609322d38cec1e0a272fe7fdea4b4a2a77b6c07))
* add correct address removal domain ([2b6c24f](https://github.com/informatievlaanderen/address-registry/commit/2b6c24f13d9e5106fa9fdd22429bb14f0521ad32))
* add correct address removal event + command ([e552dbb](https://github.com/informatievlaanderen/address-registry/commit/e552dbb473d2b226150d74bbdb636e5b82200bba))

# [4.3.0](https://github.com/informatievlaanderen/address-registry/compare/v4.2.0...v4.3.0) (2024-04-30)


### Features

* revert remove crab from extract GAWR-4677 ([d249220](https://github.com/informatievlaanderen/address-registry/commit/d249220f912a22a3b233bbb3c2f840a577faf450))

# [4.2.0](https://github.com/informatievlaanderen/address-registry/compare/v4.1.0...v4.2.0) (2024-04-29)


### Features

* add combined index isremoved and status on integration projections ([d9e0dd6](https://github.com/informatievlaanderen/address-registry/commit/d9e0dd6e3dd1d0a6506bc3746d5fed1523396366))

# [4.1.0](https://github.com/informatievlaanderen/address-registry/compare/v4.0.8...v4.1.0) (2024-04-22)


### Features

* GAWR-5509 add endpoint to get postalcode-streetname links ([e1e7aa7](https://github.com/informatievlaanderen/address-registry/commit/e1e7aa78e9f57b3bb93d6824a732029eeb3482fb))
* remove crab from extract GAWR-4677 ([1be57cd](https://github.com/informatievlaanderen/address-registry/commit/1be57cdc1352b2d713d66f8e9b3645ef990083cb))

## [4.0.8](https://github.com/informatievlaanderen/address-registry/compare/v4.0.7...v4.0.8) (2024-04-09)


### Bug Fixes

* build cachewarmer ([f56cd52](https://github.com/informatievlaanderen/address-registry/commit/f56cd52ad87793120b6a0646bfb07c22b8712c9d))
* change adresmatch V2 to with parent ([d344709](https://github.com/informatievlaanderen/address-registry/commit/d344709a3bed6af50a364a03653cafe3197b375a))

## [4.0.7](https://github.com/informatievlaanderen/address-registry/compare/v4.0.6...v4.0.7) (2024-04-08)

## [4.0.6](https://github.com/informatievlaanderen/address-registry/compare/v4.0.5...v4.0.6) (2024-03-29)


### Bug Fixes

* postinfo nullability dont emit ([1e07b1f](https://github.com/informatievlaanderen/address-registry/commit/1e07b1fe1fe2a477fedc752d5b6067a1a5965fb6))

## [4.0.5](https://github.com/informatievlaanderen/address-registry/compare/v4.0.4...v4.0.5) (2024-03-29)


### Bug Fixes

* oslo detail contract postinfo allownull ([f903b11](https://github.com/informatievlaanderen/address-registry/commit/f903b117b35545a9f5386c25575581581c321001))

## [4.0.4](https://github.com/informatievlaanderen/address-registry/compare/v4.0.3...v4.0.4) (2024-03-25)


### Bug Fixes

* bump sqs package ([7093048](https://github.com/informatievlaanderen/address-registry/commit/7093048fddd350e3f628604e8f568f6e28cbe749))

## [4.0.3](https://github.com/informatievlaanderen/address-registry/compare/v4.0.2...v4.0.3) (2024-03-25)


### Bug Fixes

* bump to trigger build ([ed9c1d7](https://github.com/informatievlaanderen/address-registry/commit/ed9c1d71cb8bc92892f8a3d560898ec94366f876))

## [4.0.2](https://github.com/informatievlaanderen/address-registry/compare/v4.0.1...v4.0.2) (2024-03-25)

## [4.0.1](https://github.com/informatievlaanderen/address-registry/compare/v4.0.0...v4.0.1) (2024-03-21)


### Bug Fixes

* AmbiguousActionException ([ae42ef7](https://github.com/informatievlaanderen/address-registry/commit/ae42ef73109fe2b05844430f1785a1756864c2a5))
* **ci:** correct version ecr-login ([1b9bcd4](https://github.com/informatievlaanderen/address-registry/commit/1b9bcd4599846b6125aec66b5cd3ae0ea05931cc))

# [4.0.0](https://github.com/informatievlaanderen/address-registry/compare/v3.116.2...v4.0.0) (2024-03-21)


### Features

* move to dotnet 8.0.2 ([e104c83](https://github.com/informatievlaanderen/address-registry/commit/e104c839b680a155c66b35768e1a168a7dc003ff))


### BREAKING CHANGES

* move to dotnet 8.0.2

## [3.116.2](https://github.com/informatievlaanderen/address-registry/compare/v3.116.1...v3.116.2) (2024-03-19)


### Bug Fixes

* limit addressmatch range to 100 ([da65684](https://github.com/informatievlaanderen/address-registry/commit/da656840a49d4a9f0ba4e0ad6af35470165c7f9d))

## [3.116.1](https://github.com/informatievlaanderen/address-registry/compare/v3.116.0...v3.116.1) (2024-03-04)


### Bug Fixes

* add parentpersistentlocalid to integration ([e533f17](https://github.com/informatievlaanderen/address-registry/commit/e533f178d89e517ab4bc755d05abd6a7cdd08bac))

# [3.116.0](https://github.com/informatievlaanderen/address-registry/compare/v3.115.0...v3.116.0) (2024-03-01)


### Features

* remove bosa functionality ([a2c70d8](https://github.com/informatievlaanderen/address-registry/commit/a2c70d82c9faf778e7fb5463420872ab11224257))

# [3.115.0](https://github.com/informatievlaanderen/address-registry/compare/v3.114.1...v3.115.0) (2024-02-22)


### Features

* add type to latest versions integration ([0a0a792](https://github.com/informatievlaanderen/address-registry/commit/0a0a792396e6014c87cf9bc9c67ed35ab732ab33))

## [3.114.1](https://github.com/informatievlaanderen/address-registry/compare/v3.114.0...v3.114.1) (2024-02-21)


### Bug Fixes

* **bump:** use new lambda pipeline ([dd92290](https://github.com/informatievlaanderen/address-registry/commit/dd922908acf2e3d997ad06e532b84e44a5235640))

# [3.114.0](https://github.com/informatievlaanderen/address-registry/compare/v3.113.1...v3.114.0) (2024-02-20)


### Features

* add ignore data check when set offset ([0c8a055](https://github.com/informatievlaanderen/address-registry/commit/0c8a05540b62f015eef70b5f9ae65ed4ae99e65a))

## [3.113.1](https://github.com/informatievlaanderen/address-registry/compare/v3.113.0...v3.113.1) (2024-02-19)


### Bug Fixes

* add delay to start backofficeprojection health check ([ad1b511](https://github.com/informatievlaanderen/address-registry/commit/ad1b511c839fc8c104b0cd0a76c4d298d5098a7e))

# [3.113.0](https://github.com/informatievlaanderen/address-registry/compare/v3.112.0...v3.113.0) (2024-02-19)


### Bug Fixes

* add consumer offset option ([4f03ad2](https://github.com/informatievlaanderen/address-registry/commit/4f03ad21897e380356c49dd837d69266db428b43))
* correct address match streetname abbreviations ([336c031](https://github.com/informatievlaanderen/address-registry/commit/336c0314591344828dec5492a45d70fae8e6dfab))


### Features

* shutdown backoffice projections service when projection is stopped ([9e01417](https://github.com/informatievlaanderen/address-registry/commit/9e01417d83c51910c8c5b080c12bb0bd8565ea53))

# [3.112.0](https://github.com/informatievlaanderen/address-registry/compare/v3.111.1...v3.112.0) (2024-02-14)


### Bug Fixes

* cache controller ([2508e9d](https://github.com/informatievlaanderen/address-registry/commit/2508e9da983a396c40320e0971ffedb5625738bf))


### Features

* add projections lastchangedlist console ([8b29da1](https://github.com/informatievlaanderen/address-registry/commit/8b29da1fc94010168ca5d2b664cec96dbbb1a0f2))

## [3.111.1](https://github.com/informatievlaanderen/address-registry/compare/v3.111.0...v3.111.1) (2024-02-08)


### Bug Fixes

* **bump:** ci correct push images ([2e41e98](https://github.com/informatievlaanderen/address-registry/commit/2e41e9879c596c583a42c98144f2b28357993c18))

# [3.111.0](https://github.com/informatievlaanderen/address-registry/compare/v3.110.1...v3.111.0) (2024-02-08)


### Bug Fixes

* integration projections ([7ff8cb1](https://github.com/informatievlaanderen/address-registry/commit/7ff8cb10d23be988770661887fcdf47b4917e71a))
* push images to new ECR ([49310fb](https://github.com/informatievlaanderen/address-registry/commit/49310fbf449de55fda6001ec24a3426ba07207c2))


### Features

* add status for backoffice projections ([502488e](https://github.com/informatievlaanderen/address-registry/commit/502488e6f9ec14f02d3247d45449151dc52beffc))

## [3.110.1](https://github.com/informatievlaanderen/address-registry/compare/v3.110.0...v3.110.1) (2024-02-06)


### Bug Fixes

* bump projection handling ([b0e0eb0](https://github.com/informatievlaanderen/address-registry/commit/b0e0eb0a7065522a4ebfa4ab2215f8f18158e132))

# [3.110.0](https://github.com/informatievlaanderen/address-registry/compare/v3.109.2...v3.110.0) (2024-02-02)


### Bug Fixes

* add spatial index on AddressWmsV2 ([c1b209f](https://github.com/informatievlaanderen/address-registry/commit/c1b209f5af5781bb2ca65f21b3a5432412c63788))


### Features

* add cachevalidator lastchangedlist GAWR-5407 ([5be734b](https://github.com/informatievlaanderen/address-registry/commit/5be734b0fb7720535e8cb3374ebaaca4d2551e5c))

## [3.109.2](https://github.com/informatievlaanderen/address-registry/compare/v3.109.1...v3.109.2) (2024-01-31)


### Bug Fixes

* docs AdresDetailHuisnummerObject ([aa57e0d](https://github.com/informatievlaanderen/address-registry/commit/aa57e0dd4e8ce30fc6fe9525f942e33a49400a9f))

## [3.109.1](https://github.com/informatievlaanderen/address-registry/compare/v3.109.0...v3.109.1) (2024-01-31)


### Bug Fixes

* add datacontract on AdresDetailHuisnummerObject ([28a0ee9](https://github.com/informatievlaanderen/address-registry/commit/28a0ee93c743f80073f35cf96cdd2d00bde5a176))

# [3.109.0](https://github.com/informatievlaanderen/address-registry/compare/v3.108.0...v3.109.0) (2024-01-31)


### Features

* add AddressDetailHuisnummer GAWR-3809 ([32f0ffc](https://github.com/informatievlaanderen/address-registry/commit/32f0ffc6f3f3918800ee9a440a930b43933e5f38))

# [3.108.0](https://github.com/informatievlaanderen/address-registry/compare/v3.107.0...v3.108.0) (2024-01-29)


### Bug Fixes

* add objectid to idempotencekey snapshot oslo ([da89e72](https://github.com/informatievlaanderen/address-registry/commit/da89e72725750f622e33450757e3285fd1adaefe))


### Features

* add address id persistentlocalid relation projections integrationdb ([b5bfcfb](https://github.com/informatievlaanderen/address-registry/commit/b5bfcfbb88f4f34c0f5e826cfee41b5ea14c1a22))

# [3.107.0](https://github.com/informatievlaanderen/address-registry/compare/v3.106.2...v3.107.0) (2024-01-19)


### Bug Fixes

* integration ksql niscode to int ([fb8fdca](https://github.com/informatievlaanderen/address-registry/commit/fb8fdca0de162c13be0a72c96c00dd3b9a771488))
* wms address housenumberlabel ([b5496c4](https://github.com/informatievlaanderen/address-registry/commit/b5496c4d3ac3181bf0a5e69b892d9c2e48e7cca7))


### Features

* add integration projections GAWR-4535 ([ccd35e1](https://github.com/informatievlaanderen/address-registry/commit/ccd35e1129f14cd298bd96684e3be24998e20b06))


### Performance Improvements

* add wms index ([aecbc6e](https://github.com/informatievlaanderen/address-registry/commit/aecbc6ec2c27d1562f31390678fcca77b97975b5))

## [3.106.2](https://github.com/informatievlaanderen/address-registry/compare/v3.106.1...v3.106.2) (2024-01-09)


### Bug Fixes

* Add AddressWmsItemV2 EF migration ([a8deefb](https://github.com/informatievlaanderen/address-registry/commit/a8deefb602f12ef7f8d86e4d0ae4d51479a81a14))

## [3.106.1](https://github.com/informatievlaanderen/address-registry/compare/v3.106.0...v3.106.1) (2024-01-09)


### Bug Fixes

* command provenance should be set after idempotency check ([6e2f232](https://github.com/informatievlaanderen/address-registry/commit/6e2f2323b2ecd86b8ccc33c6890f761216674da4))
* event timestamp ([cafbbeb](https://github.com/informatievlaanderen/address-registry/commit/cafbbeb7a1b94be35bb2a35b3704455e316cc1c0))

# [3.106.0](https://github.com/informatievlaanderen/address-registry/compare/v3.105.0...v3.106.0) (2024-01-08)


### Bug Fixes

* cast niscode to int in integrationdb stream ([379ca96](https://github.com/informatievlaanderen/address-registry/commit/379ca96765b05a0eec157e807ba326fd7d3ebd3f))
* readdress addresses box number bug ([2673b5e](https://github.com/informatievlaanderen/address-registry/commit/2673b5ef44dd63ac5e36b73b1dec7aa5d46f313c))
* use different scope for each address to retire in other streetname ([0c3e139](https://github.com/informatievlaanderen/address-registry/commit/0c3e139fd4a06e29e79e37b970971469c732e7f4))


### Features

* add idempotencekey to ksql ([b812b58](https://github.com/informatievlaanderen/address-registry/commit/b812b58f00afdd2a4f1e02d82ff158d1f33f7fad))
* loosen box number regex format ([3ab0139](https://github.com/informatievlaanderen/address-registry/commit/3ab0139ce5be2f63a7aebfa8ca4425fb88f87a31))
* loosen house number regex format for interne bijwerker ([bfac095](https://github.com/informatievlaanderen/address-registry/commit/bfac0958893dbd7e76580baff9ef45b5d28a1667))

# [3.105.0](https://github.com/informatievlaanderen/address-registry/compare/v3.104.0...v3.105.0) (2023-12-05)


### Bug Fixes

* add AddressDetailWithParent projection to projector ([b6b994c](https://github.com/informatievlaanderen/address-registry/commit/b6b994cfe1f167306befa8dfbea7624ba4c01229))
* add addressmatch extensions ([a5d1030](https://github.com/informatievlaanderen/address-registry/commit/a5d1030216eec244ae1da0a0faf95fb9c618cde8))
* versiontimestamp parsing in ksql integrationdb stream ([002469a](https://github.com/informatievlaanderen/address-registry/commit/002469aa0c5e2e4d6ddb45502cb3f6c993341bd8))
* wms labeltype by duplicating projection (V2) ([56ca872](https://github.com/informatievlaanderen/address-registry/commit/56ca872123992bed19582f2f00de6578b81c9a8a))


### Features

* add integrationdb ksql scripts ([65064ec](https://github.com/informatievlaanderen/address-registry/commit/65064ecbc1cb1d4ce78f754a9da371120d861682))
* add StreetNameWasRenamed event ([79cd967](https://github.com/informatievlaanderen/address-registry/commit/79cd967708b1623190f7e99affabde88a22ffbda))
* consume rename streetname - readdress ([00d4f8e](https://github.com/informatievlaanderen/address-registry/commit/00d4f8e259146d4668268a7ea849be85ecca96ae))
* consume rename streetname - retire and reject ([ff4f214](https://github.com/informatievlaanderen/address-registry/commit/ff4f214b9a863c6d24f71b06cc901c27e3c2f2a2))
* use IdempotentCommandHandler in streetname consumer ([9b85e4b](https://github.com/informatievlaanderen/address-registry/commit/9b85e4b9a3d19cdb8be1e3116a37164a3d8e9783))


### Performance Improvements

* **adresmatch:** various performance improvements ([6123621](https://github.com/informatievlaanderen/address-registry/commit/61236217a0ae946c4aeca35ec9d205786c396d92))

# [3.104.0](https://github.com/informatievlaanderen/address-registry/compare/v3.103.1...v3.104.0) (2023-11-07)


### Features

* disable unused syndication projections ([776fabd](https://github.com/informatievlaanderen/address-registry/commit/776fabd621f4b55c918ddef688e57e5880260018))
* switch wfs view to new tables GAWR-5211 ([cc55513](https://github.com/informatievlaanderen/address-registry/commit/cc555130aec26de147d599f910a831f0909bc6e2))
* switch wms view to new tables GAWR-4041 ([d9b7b4f](https://github.com/informatievlaanderen/address-registry/commit/d9b7b4f22e1223c233dae69f6a265e398d8fca93))

## [3.103.1](https://github.com/informatievlaanderen/address-registry/compare/v3.103.0...v3.103.1) (2023-10-17)


### Bug Fixes

* **migrator:** migrate housenumber of parent GAWR-5283 ([0fdc04f](https://github.com/informatievlaanderen/address-registry/commit/0fdc04f0d6457432aea8cde03b9c2a3ca6f5c45f))

# [3.103.0](https://github.com/informatievlaanderen/address-registry/compare/v3.102.2...v3.103.0) (2023-10-16)


### Bug Fixes

* add position to persistentlocalid syndication filter ([7eb8f04](https://github.com/informatievlaanderen/address-registry/commit/7eb8f04501e1119fd348fe7e412d6c04dca6839a))
* wms change labeltype house-boxnumbers ([1c24890](https://github.com/informatievlaanderen/address-registry/commit/1c24890ed0574e46319eeb262763cec071f8a169))


### Features

* add AddressDetailItemV2WithParent ([22c3d04](https://github.com/informatievlaanderen/address-registry/commit/22c3d049b6e9f503d8ee111839e71002811753f5))

## [3.102.2](https://github.com/informatievlaanderen/address-registry/compare/v3.102.1...v3.102.2) (2023-10-09)


### Bug Fixes

* WMS address label type ([110b3ae](https://github.com/informatievlaanderen/address-registry/commit/110b3ae59bf70d0f8201396fa80015c797c36427))

## [3.102.1](https://github.com/informatievlaanderen/address-registry/compare/v3.102.0...v3.102.1) (2023-10-04)


### Bug Fixes

* push snapshot verifier image to new prd ([efd7cc1](https://github.com/informatievlaanderen/address-registry/commit/efd7cc106647530a778c14c52d855655eb70c29e))

# [3.102.0](https://github.com/informatievlaanderen/address-registry/compare/v3.101.9...v3.102.0) (2023-10-03)


### Bug Fixes

* add snapshot-verifier to release ([08feee8](https://github.com/informatievlaanderen/address-registry/commit/08feee8969a60b05ce4d01209f6da9fc3005c406))
* remove sequences from snapshot verifier ([5809ce4](https://github.com/informatievlaanderen/address-registry/commit/5809ce45fc5419c85b5d420fcde6290afcefec9b))
* snapshot verifier ([f62ee89](https://github.com/informatievlaanderen/address-registry/commit/f62ee895078422376b43a11797f67ed8a695bebc))


### Features

* add feed endpoint filtering on persistentLocalId ([2cdb061](https://github.com/informatievlaanderen/address-registry/commit/2cdb06114090e56570a7d16de26267d769c6b503))

## [3.101.9](https://github.com/informatievlaanderen/address-registry/compare/v3.101.8...v3.101.9) (2023-09-04)


### Bug Fixes

* bump lambda packages ([4da6622](https://github.com/informatievlaanderen/address-registry/commit/4da662214ba0957161bc5a5ab5b7dbef973cff86))

## [3.101.8](https://github.com/informatievlaanderen/address-registry/compare/v3.101.7...v3.101.8) (2023-09-04)


### Bug Fixes

* bump lambda package ([a3d64a5](https://github.com/informatievlaanderen/address-registry/commit/a3d64a503af1d7a0dbb5bae54353d780f6c13fa1))
* event description ([097a98c](https://github.com/informatievlaanderen/address-registry/commit/097a98c13bddc4bdf99faa15f3dd68ef25b7e792))

## [3.101.7](https://github.com/informatievlaanderen/address-registry/compare/v3.101.6...v3.101.7) (2023-08-29)


### Bug Fixes

* **migrator:** don't insert twice in processeids when retrying [#2](https://github.com/informatievlaanderen/address-registry/issues/2) ([8acffe5](https://github.com/informatievlaanderen/address-registry/commit/8acffe5d4094ab2e428d9f1ca6f5d577cac2b709))

## [3.101.6](https://github.com/informatievlaanderen/address-registry/compare/v3.101.5...v3.101.6) (2023-08-29)


### Bug Fixes

* **migrator:** don't insert twice in processeids when retrying ([325acd2](https://github.com/informatievlaanderen/address-registry/commit/325acd2517a57a9baa6f4a5c43532aab4aac3769))

## [3.101.5](https://github.com/informatievlaanderen/address-registry/compare/v3.101.4...v3.101.5) (2023-08-29)


### Bug Fixes

* **migrator:** no double processing when retrying ([f257ffe](https://github.com/informatievlaanderen/address-registry/commit/f257ffecfe5f36eaaed375295074161482bc04ae))

## [3.101.4](https://github.com/informatievlaanderen/address-registry/compare/v3.101.3...v3.101.4) (2023-08-29)


### Bug Fixes

* **migrator:** add child to processed items ([ebb2c92](https://github.com/informatievlaanderen/address-registry/commit/ebb2c92decd68828618b185fff27ad8cd1d82c23))

## [3.101.3](https://github.com/informatievlaanderen/address-registry/compare/v3.101.2...v3.101.3) (2023-08-29)


### Bug Fixes

* **migrator:** add retry logic when parent not found ([21ab61b](https://github.com/informatievlaanderen/address-registry/commit/21ab61b9886b7bb697a70dfd8f26f4edc54740f1))

## [3.101.2](https://github.com/informatievlaanderen/address-registry/compare/v3.101.1...v3.101.2) (2023-08-28)


### Bug Fixes

* **migrator:** incomplete is only when missing geometry and/or status ([d7dd029](https://github.com/informatievlaanderen/address-registry/commit/d7dd02935b237663aabe07c42e24dbf1fa44fc0d))

## [3.101.1](https://github.com/informatievlaanderen/address-registry/compare/v3.101.0...v3.101.1) (2023-08-28)


### Bug Fixes

* incomplete nonflemish address ([c796b26](https://github.com/informatievlaanderen/address-registry/commit/c796b26dfcf0dc45ce513c4ca2414867c6167f50))

# [3.101.0](https://github.com/informatievlaanderen/address-registry/compare/v3.100.0...v3.101.0) (2023-08-28)


### Bug Fixes

* remove elastic apm from appsettings snapshot verifier ([677b8a7](https://github.com/informatievlaanderen/address-registry/commit/677b8a78110628c3521b85f76aa00182aff481ea))
* test region in migrator to skip address ([40fcef5](https://github.com/informatievlaanderen/address-registry/commit/40fcef552cd9e359966a65d5ec7e9763f0f8fdb9))


### Features

* add snapshot verifier ([537f09d](https://github.com/informatievlaanderen/address-registry/commit/537f09dfec57f35590112150e96213df6950aca9))
* add spatial index wfs v2 ([4de5f0c](https://github.com/informatievlaanderen/address-registry/commit/4de5f0cd91e523e586c7a33bd3bea6c1edd92604))

# [3.100.0](https://github.com/informatievlaanderen/address-registry/compare/v3.99.10...v3.100.0) (2023-08-21)


### Features

* migrate inconsistent object - empty postalcode of boxnumber ([4e98da0](https://github.com/informatievlaanderen/address-registry/commit/4e98da094aaac22d621bc2a449221b8e26e72d0d))
* migrate inconsistent object - not officially assigned ([ed162d0](https://github.com/informatievlaanderen/address-registry/commit/ed162d0120e026a2fce4a7cd174fb84256be2b40))
* migrate inconsistent object - retired streetname ([3dc63d3](https://github.com/informatievlaanderen/address-registry/commit/3dc63d39772671ecbfc630a2c90e849ad0e65af5))

## [3.99.10](https://github.com/informatievlaanderen/address-registry/compare/v3.99.9...v3.99.10) (2023-08-21)


### Bug Fixes

* call cancel on lambda cancellationToken after 5 minutes ([10034fa](https://github.com/informatievlaanderen/address-registry/commit/10034fa675c60e925f65f08daebda4636bd723e2))

## [3.99.9](https://github.com/informatievlaanderen/address-registry/compare/v3.99.8...v3.99.9) (2023-08-14)


### Bug Fixes

* style to trigger build ([1a7c829](https://github.com/informatievlaanderen/address-registry/commit/1a7c82934c6c4a60a3ed398c3dbba225c28b3ce5))

## [3.99.8](https://github.com/informatievlaanderen/address-registry/compare/v3.99.7...v3.99.8) (2023-08-11)


### Bug Fixes

* style to trigger build ([e508395](https://github.com/informatievlaanderen/address-registry/commit/e508395c311a602ba4d6d95eabe63bc2d6cf2736))

## [3.99.7](https://github.com/informatievlaanderen/address-registry/compare/v3.99.6...v3.99.7) (2023-08-08)


### Bug Fixes

* change how lambda is deployed in test ([e5ab33d](https://github.com/informatievlaanderen/address-registry/commit/e5ab33dc4a05e023cd811d0191bba49462ffbcd9))

## [3.99.6](https://github.com/informatievlaanderen/address-registry/compare/v3.99.5...v3.99.6) (2023-06-22)


### Bug Fixes

* register lazy peristentlocalidgenerator on consumer streetname ([8707054](https://github.com/informatievlaanderen/address-registry/commit/8707054094d6c2dc97d56e6a87cd845aaa1a0920))

## [3.99.5](https://github.com/informatievlaanderen/address-registry/compare/v3.99.4...v3.99.5) (2023-06-22)


### Bug Fixes

* naming bosa consumer + connected projection name ([ab6248d](https://github.com/informatievlaanderen/address-registry/commit/ab6248dddbc24253c0337ca7a30949dccd803e72))
* remove elastic npm ([a99ba85](https://github.com/informatievlaanderen/address-registry/commit/a99ba855feb49bffeb4012f0d15af9768bc6d27e))

## [3.99.4](https://github.com/informatievlaanderen/address-registry/compare/v3.99.3...v3.99.4) (2023-06-13)


### Bug Fixes

* add idempotent streetname-address relation ([6fbf1f3](https://github.com/informatievlaanderen/address-registry/commit/6fbf1f333e15b3bad6e77404417bd8907bfb3b3f))

## [3.99.3](https://github.com/informatievlaanderen/address-registry/compare/v3.99.2...v3.99.3) (2023-06-13)


### Bug Fixes

* style to trigger build ([d1c9cdf](https://github.com/informatievlaanderen/address-registry/commit/d1c9cdf50b2e4bc258c69d3f7a3038dec907712e))

## [3.99.2](https://github.com/informatievlaanderen/address-registry/compare/v3.99.1...v3.99.2) (2023-06-12)


### Bug Fixes

* projector name snapshot oslo ([098673a](https://github.com/informatievlaanderen/address-registry/commit/098673a1c73fc2aa91f9b629f57c29e251ef30c8))

## [3.99.1](https://github.com/informatievlaanderen/address-registry/compare/v3.99.0...v3.99.1) (2023-06-12)


### Bug Fixes

* update ci prepare lambda newprd ([467241d](https://github.com/informatievlaanderen/address-registry/commit/467241dd1d6f5c08da89d84a2f46e7d3831a4173))

# [3.99.0](https://github.com/informatievlaanderen/address-registry/compare/v3.98.4...v3.99.0) (2023-06-12)


### Bug Fixes

* broken build projector ([21ee11f](https://github.com/informatievlaanderen/address-registry/commit/21ee11f6f32737fe85d97a0037d1925609370473))
* bump address retries migrator ([eb6a546](https://github.com/informatievlaanderen/address-registry/commit/eb6a54660af7922465c639dbcc85c81e3fcc4ba7))
* returning only date on status controller consumer ([efceda4](https://github.com/informatievlaanderen/address-registry/commit/efceda48d969c0f903b5077d2410bf33a6cac6bb))


### Features

* add ConsumersController ([dbcb839](https://github.com/informatievlaanderen/address-registry/commit/dbcb8391a1cd640ea18ab8466a2b0186206fc729))

## [3.98.4](https://github.com/informatievlaanderen/address-registry/compare/v3.98.3...v3.98.4) (2023-05-31)


### Bug Fixes

* nullable postalcode from snapshot ([a9f0ad5](https://github.com/informatievlaanderen/address-registry/commit/a9f0ad551195e0c5cf24b45ea69e46773ed628eb))

## [3.98.3](https://github.com/informatievlaanderen/address-registry/compare/v3.98.2...v3.98.3) (2023-05-31)


### Bug Fixes

* pass StreetNameConsumerItem collection to StreamMigrator instead of querying upon each retry" ([46ec348](https://github.com/informatievlaanderen/address-registry/commit/46ec348c15a03cd3334c2bbcc7f7c51bb2de92cb))

## [3.98.2](https://github.com/informatievlaanderen/address-registry/compare/v3.98.1...v3.98.2) (2023-05-31)


### Bug Fixes

* consumer items to concurrent bag migrator ([967b168](https://github.com/informatievlaanderen/address-registry/commit/967b16891404d4bf8caed554df0e4761668a9680))

## [3.98.1](https://github.com/informatievlaanderen/address-registry/compare/v3.98.0...v3.98.1) (2023-05-31)


### Bug Fixes

* ioc registration address migratorr ([aafcc67](https://github.com/informatievlaanderen/address-registry/commit/aafcc6740ceb3c3258757eca40754b1c7e99cefc))

# [3.98.0](https://github.com/informatievlaanderen/address-registry/compare/v3.97.1...v3.98.0) (2023-05-30)


### Features

* run migrator in parallel ([34d5e1f](https://github.com/informatievlaanderen/address-registry/commit/34d5e1fc65066ab99b2fd58f3e5ed1f02ced4d28))

## [3.97.1](https://github.com/informatievlaanderen/address-registry/compare/v3.97.0...v3.97.1) (2023-05-29)


### Bug Fixes

* add CI/CD newprd ([8938dde](https://github.com/informatievlaanderen/address-registry/commit/8938ddea43010cac8afe59c403c740beb63a11e0))

# [3.97.0](https://github.com/informatievlaanderen/address-registry/compare/v3.96.3...v3.97.0) (2023-05-26)


### Features

* add elastic apm ([b9d3ca0](https://github.com/informatievlaanderen/address-registry/commit/b9d3ca08c1babe66e3debc93e22f13bfccbb1757))


### Performance Improvements

* encapsulate list and use dictionary lookups for streetnameaddresses ([e6068cf](https://github.com/informatievlaanderen/address-registry/commit/e6068cff5ffa8a1334ac4f0a5580a28ce2eb6268))

## [3.96.3](https://github.com/informatievlaanderen/address-registry/compare/v3.96.2...v3.96.3) (2023-05-19)


### Bug Fixes

* return correct etags when retiring addresses from different streetnames during readdress GAWR-4850 ([fe5b187](https://github.com/informatievlaanderen/address-registry/commit/fe5b18731f9896bd228ccc36c12c114f4e2f0f9c))
* set last event on box number addresses when applying AddressHouseNumberWasReaddressed ([d65594f](https://github.com/informatievlaanderen/address-registry/commit/d65594fb454be0153305d795cc48250966868eb6))

## [3.96.2](https://github.com/informatievlaanderen/address-registry/compare/v3.96.1...v3.96.2) (2023-05-17)


### Bug Fixes

* readdress idempotency with addresses to retire GAWR-4853 ([e2045d9](https://github.com/informatievlaanderen/address-registry/commit/e2045d9375598aa725b6d020c99c2618eb8db8c1))

## [3.96.1](https://github.com/informatievlaanderen/address-registry/compare/v3.96.0...v3.96.1) (2023-05-16)


### Bug Fixes

* identical source and destination house numbers bug ([2546579](https://github.com/informatievlaanderen/address-registry/commit/2546579442fdebb868b26edaa2c885f7bff58506))

# [3.96.0](https://github.com/informatievlaanderen/address-registry/compare/v3.95.10...v3.96.0) (2023-05-12)


### Features

* add spatial index for wms v2 ([57e034f](https://github.com/informatievlaanderen/address-registry/commit/57e034fe26114b1f324656e37c7cbfa5de2cbfd8))

## [3.95.10](https://github.com/informatievlaanderen/address-registry/compare/v3.95.9...v3.95.10) (2023-05-12)


### Bug Fixes

* style to trigger build ([12273bd](https://github.com/informatievlaanderen/address-registry/commit/12273bdde5009c61404686704b6f8fcd95d37ac7))

## [3.95.9](https://github.com/informatievlaanderen/address-registry/compare/v3.95.8...v3.95.9) (2023-05-10)


### Bug Fixes

* add savechanges in lambda after readdress ([c3e136f](https://github.com/informatievlaanderen/address-registry/commit/c3e136f24cbb078af4503e492165d571ae797f5f))

## [3.95.8](https://github.com/informatievlaanderen/address-registry/compare/v3.95.7...v3.95.8) (2023-05-10)


### Bug Fixes

* snapshotmanager with etag comparison ([0f9ec6a](https://github.com/informatievlaanderen/address-registry/commit/0f9ec6ae92e4ee041bf6728fdabc25d4ef2d172c))

## [3.95.7](https://github.com/informatievlaanderen/address-registry/compare/v3.95.6...v3.95.7) (2023-05-09)


### Bug Fixes

* docs events ([84b537c](https://github.com/informatievlaanderen/address-registry/commit/84b537cd0f948c560197025fc51967d2c0ef52fe))
* retire housenumber event after readdress housenumber event ([3ac3fdd](https://github.com/informatievlaanderen/address-registry/commit/3ac3fdda68066fdc8531b30cf22e07450abc92cb))

## [3.95.6](https://github.com/informatievlaanderen/address-registry/compare/v3.95.5...v3.95.6) (2023-05-08)


### Bug Fixes

* change docs GAWR-3711 ([59d55a6](https://github.com/informatievlaanderen/address-registry/commit/59d55a6d56837a681a9639e2c3829f922f0db2ff))
* docs events GAWR-4829 GAWR-4824 ([13b059b](https://github.com/informatievlaanderen/address-registry/commit/13b059bc4fa3cc1b647c4c6206fc3ce263f4dc7b))

## [3.95.5](https://github.com/informatievlaanderen/address-registry/compare/v3.95.4...v3.95.5) (2023-05-03)


### Bug Fixes

* AddressWasProposedBecauseOfReaddress description ([784e625](https://github.com/informatievlaanderen/address-registry/commit/784e6259735eab96b626109586a91b7efbc2df7b))
* duplication syndication key ([f24cd8a](https://github.com/informatievlaanderen/address-registry/commit/f24cd8a517d2dbddadeabaae674ecc82ea213e08))

## [3.95.4](https://github.com/informatievlaanderen/address-registry/compare/v3.95.3...v3.95.4) (2023-05-03)


### Bug Fixes

* address status error code when correcting retirment ([3bc1467](https://github.com/informatievlaanderen/address-registry/commit/3bc1467788dbce4fd01c1c7cec48a414533374b9))
* replace NBSP spaces by normal spaces ([39e1c95](https://github.com/informatievlaanderen/address-registry/commit/39e1c951d4f9c2a2abf95a789545a501db3ae6dc))

## [3.95.3](https://github.com/informatievlaanderen/address-registry/compare/v3.95.2...v3.95.3) (2023-04-26)


### Bug Fixes

* address puri in readdress error messages ([89255df](https://github.com/informatievlaanderen/address-registry/commit/89255dfb6cb49bd01f0747011e5faca993516582))

## [3.95.2](https://github.com/informatievlaanderen/address-registry/compare/v3.95.1...v3.95.2) (2023-04-26)


### Bug Fixes

* consumer sequences registration + recover lost offset ([24b599b](https://github.com/informatievlaanderen/address-registry/commit/24b599b5cf56c75a9b712ee719607047c6d468fe))

## [3.95.1](https://github.com/informatievlaanderen/address-registry/compare/v3.95.0...v3.95.1) (2023-04-25)


### Bug Fixes

* another bunch of corrections for readdress error messages and codes ([befd5e3](https://github.com/informatievlaanderen/address-registry/commit/befd5e30853595c7d172a2a590c3a982e666505f))
* readdress error codes and error messages ([030f948](https://github.com/informatievlaanderen/address-registry/commit/030f9489a66af3de127d13eef78bc109f6781b85))
* remove brackets around variable in error message ([30e50a2](https://github.com/informatievlaanderen/address-registry/commit/30e50a29a063f3e603dbc16c9bbc35f87b24d711))
* sqs handlers registration ([ccca47e](https://github.com/informatievlaanderen/address-registry/commit/ccca47eb2e43503328acc9ee18656b9a70cfc236))

# [3.95.0](https://github.com/informatievlaanderen/address-registry/compare/v3.94.1...v3.95.0) (2023-04-21)


### Features

* add creatieid to extract ([fe1ec21](https://github.com/informatievlaanderen/address-registry/commit/fe1ec21ecab2fdd2c5a95d3143384163a8099052))

## [3.94.1](https://github.com/informatievlaanderen/address-registry/compare/v3.94.0...v3.94.1) (2023-04-19)


### Bug Fixes

* add readdress request example ([011f895](https://github.com/informatievlaanderen/address-registry/commit/011f895abe2ba70ae84014a0c1bd35c8a2d27d89))
* description of readdress request OpheffenAdressen ([ec6b93a](https://github.com/informatievlaanderen/address-registry/commit/ec6b93acbbf2d24d3772232ca1e6d85fdf8b30fc))
* rename readdress ticket action name ([a00fdce](https://github.com/informatievlaanderen/address-registry/commit/a00fdcefcdbd0ed53d8c741d7869759a1af3c79b))

# [3.94.0](https://github.com/informatievlaanderen/address-registry/compare/v3.93.2...v3.94.0) (2023-04-18)


### Bug Fixes

* remove AddressHouseNumberWasReplacedBecauseOfReaddress ([369300c](https://github.com/informatievlaanderen/address-registry/commit/369300c9364ad319d0f43c1b5f60d40c2905ef78))
* remove rejected and retired boxnumbers from AddressHouseNumberWasReaddressed ([0f76cbf](https://github.com/informatievlaanderen/address-registry/commit/0f76cbfc0a7f94a7aa31995dcaef260e424a24ff))
* rename readdressing to readdress ([5ec3a54](https://github.com/informatievlaanderen/address-registry/commit/5ec3a5434e3468676071eae3ac347b28d9fbc0ee))


### Features

* projections for new readdress events ([839a095](https://github.com/informatievlaanderen/address-registry/commit/839a095133a789bc4a516c8476e43b6f54c644ff))

## [3.93.2](https://github.com/informatievlaanderen/address-registry/compare/v3.93.1...v3.93.2) (2023-04-13)

## [3.93.1](https://github.com/informatievlaanderen/address-registry/compare/v3.93.0...v3.93.1) (2023-04-13)


### Bug Fixes

* case insensitive housenumber and boxnumber comparison ([4099be9](https://github.com/informatievlaanderen/address-registry/commit/4099be961e225ec339519ae11a3ac48c89a20f2e))
* don't put migrated removed address in extract ([62a8371](https://github.com/informatievlaanderen/address-registry/commit/62a8371bd118723fe632b3cf705b2fa3fe78decc))

# [3.93.0](https://github.com/informatievlaanderen/address-registry/compare/v3.92.1...v3.93.0) (2023-04-12)


### Features

* add gemeenteid to extract ([258ca31](https://github.com/informatievlaanderen/address-registry/commit/258ca313f9be63b358ec1bca475796e4a62504d9))

## [3.92.1](https://github.com/informatievlaanderen/address-registry/compare/v3.92.0...v3.92.1) (2023-04-11)


### Bug Fixes

* remove removed addresses from extract ([46d2185](https://github.com/informatievlaanderen/address-registry/commit/46d2185192fd43de742910d12268c5dc0e019f95))

# [3.92.0](https://github.com/informatievlaanderen/address-registry/compare/v3.91.0...v3.92.0) (2023-04-07)


### Features

* readdress projections ([8f33254](https://github.com/informatievlaanderen/address-registry/commit/8f332540215702bea88dee79dcdc148b8b8e1d53))
* split crab extract in seperate call ([78c8ec6](https://github.com/informatievlaanderen/address-registry/commit/78c8ec6f537bcdd503b56629e4760120ab51b307))

# [3.91.0](https://github.com/informatievlaanderen/address-registry/compare/v3.90.0...v3.91.0) (2023-04-06)


### Bug Fixes

* make errorcodes unique GAWR-4709 ([c0c1b2f](https://github.com/informatievlaanderen/address-registry/commit/c0c1b2f2ff87ef8856240b84c30688038a4f4f43))
* name to DestinationParentAddressPersistentLocalId ([26127f2](https://github.com/informatievlaanderen/address-registry/commit/26127f253c5518512059b904b94b897730f11d75))


### Features

* readdress RetireAddresses list ([429f8c4](https://github.com/informatievlaanderen/address-registry/commit/429f8c43a6e8c9be7ca4036653343841a4a30325))

# [3.90.0](https://github.com/informatievlaanderen/address-registry/compare/v3.89.2...v3.90.0) (2023-04-04)


### Bug Fixes

* addressmatch nullable postalcode ([3df83bb](https://github.com/informatievlaanderen/address-registry/commit/3df83bbc86bf81274f087cc2350a3f357dcd39ee))


### Features

* readdress scenario destination address exists with boxnumbers ([dcdc19d](https://github.com/informatievlaanderen/address-registry/commit/dcdc19d356829a98448d9353f5bd1800d486fcf1))
* readdress scenario other source streetname ([2e5bad3](https://github.com/informatievlaanderen/address-registry/commit/2e5bad3a1be6be76d18935f5eb5d1c3477d673ee))
* readress validation source and destination address are not the same ([5d9d27e](https://github.com/informatievlaanderen/address-registry/commit/5d9d27e5531e49c70c6f061966825965bb7e4410))

## [3.89.2](https://github.com/informatievlaanderen/address-registry/compare/v3.89.1...v3.89.2) (2023-04-03)


### Bug Fixes

* api-backoffice non-root ([15a3e44](https://github.com/informatievlaanderen/address-registry/commit/15a3e44fe67cafc4877ba20fb171a3f7944f637b))
* run apt-get as root ([92c981c](https://github.com/informatievlaanderen/address-registry/commit/92c981c1442f0e2408ade3c2d66cb063790f974d))
* run containers as non-root ([beb879e](https://github.com/informatievlaanderen/address-registry/commit/beb879e54bdb4e1efd595f6abbeed37428aa70ec))
* use --gid instead of --group ([e4f29c2](https://github.com/informatievlaanderen/address-registry/commit/e4f29c2abca6bd44e74ba078f5b4aa10614e905e))
* use --group instead of -G ([c312c4f](https://github.com/informatievlaanderen/address-registry/commit/c312c4fda51eedb8dbddae7c54f88bf5e6d61cff))
* use --system instead of -S ([6743f43](https://github.com/informatievlaanderen/address-registry/commit/6743f43ccf252bcf7be50681fdcfe30af93be8da))
* use group number instead of variable ([8e8d851](https://github.com/informatievlaanderen/address-registry/commit/8e8d851451ab814d4add4948f17ae0ca6eb51a39))

## [3.89.1](https://github.com/informatievlaanderen/address-registry/compare/v3.89.0...v3.89.1) (2023-04-03)


### Bug Fixes

* update removed items in WMS for aggregate events ([e6c1cfe](https://github.com/informatievlaanderen/address-registry/commit/e6c1cfebc0a6aae8e77fbe05dd237936af2195c7))

# [3.89.0](https://github.com/informatievlaanderen/address-registry/compare/v3.88.0...v3.89.0) (2023-03-31)


### Features

* add vol adres to extract v2 ([0310cca](https://github.com/informatievlaanderen/address-registry/commit/0310cca79d2dfce109a2cdcdf6fcc64ac36c43a3))

# [3.88.0](https://github.com/informatievlaanderen/address-registry/compare/v3.87.0...v3.88.0) (2023-03-30)


### Features

* add wms v2 views ([a35d7a0](https://github.com/informatievlaanderen/address-registry/commit/a35d7a07c4d396b9cb269eea74f2738a1807671c))

# [3.87.0](https://github.com/informatievlaanderen/address-registry/compare/v3.86.0...v3.87.0) (2023-03-30)


### Bug Fixes

* hateoas ref to href ([a2e3786](https://github.com/informatievlaanderen/address-registry/commit/a2e3786885c87ffbb6dcce22b58da4e1c9e9ef67))


### Features

* readdress scenario destination address exists ([d33f94b](https://github.com/informatievlaanderen/address-registry/commit/d33f94b37b47fddda828cd1b87502b2a16d1e923))

# [3.86.0](https://github.com/informatievlaanderen/address-registry/compare/v3.85.0...v3.86.0) (2023-03-29)


### Bug Fixes

* register IPersistentLocalIdGenerator in lambda ([4d9d386](https://github.com/informatievlaanderen/address-registry/commit/4d9d3869456dbd2166eaf85e3769cd8ed533a10e))


### Features

* readdress first scenario with boxnumbers ([792ca8e](https://github.com/informatievlaanderen/address-registry/commit/792ca8e6067e0a811418cc9d5a2f70bd142626f8))

# [3.85.0](https://github.com/informatievlaanderen/address-registry/compare/v3.84.1...v3.85.0) (2023-03-28)


### Features

* add first scenario of readdress domain ([1f5d7b0](https://github.com/informatievlaanderen/address-registry/commit/1f5d7b035cf4f5aca77d62178a0a4bda93f7e3a5))
* add missing streetname status validations ([2224259](https://github.com/informatievlaanderen/address-registry/commit/22242594940773880cfbb6eadde2423dd6dd2afc))

## [3.84.1](https://github.com/informatievlaanderen/address-registry/compare/v3.84.0...v3.84.1) (2023-03-24)


### Bug Fixes

* type of address links sync feed ([a97a377](https://github.com/informatievlaanderen/address-registry/commit/a97a377edeabf9d268abf2ca20d05cc075cf8750))

# [3.84.0](https://github.com/informatievlaanderen/address-registry/compare/v3.83.0...v3.84.0) (2023-03-22)


### Bug Fixes

* add https ([06afe53](https://github.com/informatievlaanderen/address-registry/commit/06afe5341d45b11b2134bd9165fd40ba18f3a674))
* consume StreetNameWasRemovedV2 ([c597499](https://github.com/informatievlaanderen/address-registry/commit/c597499e95cdb885de1e8209a02b242ff45cd04e))
* correct if not ([4318a83](https://github.com/informatievlaanderen/address-registry/commit/4318a8350b95a1b455612a2950b057cbba174d6c))
* echo endpoint ([d168d23](https://github.com/informatievlaanderen/address-registry/commit/d168d23277f9d41ec52c00b233442261bfe14eae))
* echo variable ([f884fda](https://github.com/informatievlaanderen/address-registry/commit/f884fda19c015e9402bc1d10ccf0ce6440271be7))
* execute statements ([74422e1](https://github.com/informatievlaanderen/address-registry/commit/74422e1e277f592a8b565703215f68802bbd053a))
* initialize variable ([f7b968e](https://github.com/informatievlaanderen/address-registry/commit/f7b968e6cbed8353a8e916001759e5693c5ddc19))
* initialize variable ([5e20fc6](https://github.com/informatievlaanderen/address-registry/commit/5e20fc6098ad28b78493e90de8952c2512653bf4))
* no bash if not ([7755a49](https://github.com/informatievlaanderen/address-registry/commit/7755a4985cc03b22b6590674ca47075c873e8bd9))
* postalcode nullability ([c678d36](https://github.com/informatievlaanderen/address-registry/commit/c678d361d9318a8ebd0e2046f93a0a6b0d18cf48))
* prefix https ([f34e874](https://github.com/informatievlaanderen/address-registry/commit/f34e874bfbd0df56689c39decabdcfcd69e24e97))
* secrets ([2667fa0](https://github.com/informatievlaanderen/address-registry/commit/2667fa011c77f198df8b4d5dc829f5df98787152))
* secrets ([1ee1dfc](https://github.com/informatievlaanderen/address-registry/commit/1ee1dfc9033cc376dc5eea5050563a51b4e2d104))
* simplify folder processing ([d5563ef](https://github.com/informatievlaanderen/address-registry/commit/d5563ef7c3a5a3473ab603507de76f39186847ec))
* use different variable ([5f8f0b6](https://github.com/informatievlaanderen/address-registry/commit/5f8f0b6df500d288c37150835a298a09269606f2))


### Features

* add readdress duplication validations ([fe41373](https://github.com/informatievlaanderen/address-registry/commit/fe41373e58093955d91c43687f9fe551cb9733fb))
* disable toggle addressmatch v2 data in legacy ([7203a71](https://github.com/informatievlaanderen/address-registry/commit/7203a7152574df05b256ce3a643310114daef010))

# [3.83.0](https://github.com/informatievlaanderen/address-registry/compare/v3.82.0...v3.83.0) (2023-03-21)


### Bug Fixes

* add url scheme ([88624ee](https://github.com/informatievlaanderen/address-registry/commit/88624ee0b18b5b78e4a7a1b662ae0ffca04acd81))
* correct ksql.yml ([dc830e4](https://github.com/informatievlaanderen/address-registry/commit/dc830e477e24e47358bf1e4a9dadeeabe14e42b8))
* correct ksql.yml ([07e7923](https://github.com/informatievlaanderen/address-registry/commit/07e79233ccc3b2daadb2e9cddefa4ecf3a6248cb))
* curly braces around url ([8ae8f83](https://github.com/informatievlaanderen/address-registry/commit/8ae8f83388c10bacbb9086c0c5934e3dca053375))
* declare secret_value ([b23bd78](https://github.com/informatievlaanderen/address-registry/commit/b23bd782ee24c6102304a2860147177f4d56f3f2))
* fix curl auth ([6f2d9e0](https://github.com/informatievlaanderen/address-registry/commit/6f2d9e0688b6518be0c1dc28799da2025df83d53))
* fix endpoint locations ([a21d415](https://github.com/informatievlaanderen/address-registry/commit/a21d415c8303ef400cdfedb287ff9fcbbd95a52b))
* ksql.yml ([adf8019](https://github.com/informatievlaanderen/address-registry/commit/adf8019e98c2df0911646cd7af52653c474a4781))
* ksql.yml ([cd7e4ef](https://github.com/informatievlaanderen/address-registry/commit/cd7e4ef0144ac2c7557ad19373f0f2fbd13fd655))
* move environment key ([3cc897e](https://github.com/informatievlaanderen/address-registry/commit/3cc897e26c494d99f3e3f36528ca0e5328369561))
* progress to endpoint URL ([8b8a750](https://github.com/informatievlaanderen/address-registry/commit/8b8a75013350615f86d76146151b3eedefbc0053))
* progress to endpoint url [#2](https://github.com/informatievlaanderen/address-registry/issues/2) ([c4f7251](https://github.com/informatievlaanderen/address-registry/commit/c4f7251806843c25f49d88c7ad464d6b85c34566))
* remove tabs from ksql.yml ([7952382](https://github.com/informatievlaanderen/address-registry/commit/79523827053a80198b83b3be3cc127d9e136a85c))
* secrets ([eab3877](https://github.com/informatievlaanderen/address-registry/commit/eab3877889bf5c305aa6bb556345c05b3e66676b))
* secrets ([b77b8ce](https://github.com/informatievlaanderen/address-registry/commit/b77b8ce0182d51de9d8d766dd78f222bfbd50a3b))
* use different indirection ([3fcb90b](https://github.com/informatievlaanderen/address-registry/commit/3fcb90b4c7d9d59b503d66c739361d770fa226c0))
* verify ksql.yml ([13200cf](https://github.com/informatievlaanderen/address-registry/commit/13200cf85797cbed09e852d08f4b78ca47b3fe93))


### Features

* add hateoas to adresmatch ([8ac2917](https://github.com/informatievlaanderen/address-registry/commit/8ac291798374cd372e2bf3766c044be470b49e7e))
* readdress api ([b952f9b](https://github.com/informatievlaanderen/address-registry/commit/b952f9be46c0032a0ab44f680eaa13e65523a0fc))

# [3.82.0](https://github.com/informatievlaanderen/address-registry/compare/v3.81.3...v3.82.0) (2023-03-16)


### Features

* consume streetname homonym addition corrections in domain ([0bbb251](https://github.com/informatievlaanderen/address-registry/commit/0bbb25192baaae012cad3db67663343559ad6be4))

## [3.81.3](https://github.com/informatievlaanderen/address-registry/compare/v3.81.2...v3.81.3) (2023-03-15)


### Bug Fixes

* box number postal code error message on propose address ([6697aa2](https://github.com/informatievlaanderen/address-registry/commit/6697aa2fffc561a80ba75b815562aab6c29c8755))

## [3.81.2](https://github.com/informatievlaanderen/address-registry/compare/v3.81.1...v3.81.2) (2023-03-14)


### Bug Fixes

* AddressListViewV2 nullable postal code ([b725774](https://github.com/informatievlaanderen/address-registry/commit/b7257743a8744d3e0f8d0e06a2f4f347d5bf553d))

## [3.81.1](https://github.com/informatievlaanderen/address-registry/compare/v3.81.0...v3.81.1) (2023-03-14)


### Bug Fixes

* address list ([a0cd11c](https://github.com/informatievlaanderen/address-registry/commit/a0cd11c42ca5130f6f239085b85790b0dcf05aa1))
* AddressWasRemovedBecauseStreetNameWasRemoved docs ([cf59423](https://github.com/informatievlaanderen/address-registry/commit/cf5942394015ed09877ffc2e53f06054dbdef82d))

# [3.81.0](https://github.com/informatievlaanderen/address-registry/compare/v3.80.1...v3.81.0) (2023-03-14)


### Features

* add addresslist view for v2 and oslo ([a3898a5](https://github.com/informatievlaanderen/address-registry/commit/a3898a5a91bfcb557bbe62f455e508aa2eaa2ba3))

## [3.80.1](https://github.com/informatievlaanderen/address-registry/compare/v3.80.0...v3.80.1) (2023-03-14)


### Bug Fixes

* address match response context ([7598a85](https://github.com/informatievlaanderen/address-registry/commit/7598a854870d60b1f7630d00a565fe32ee769e88))

# [3.80.0](https://github.com/informatievlaanderen/address-registry/compare/v3.79.2...v3.80.0) (2023-03-13)


### Bug Fixes

* add Oslo suffix to address match V2 response classes to satisfy swagger ([431fb20](https://github.com/informatievlaanderen/address-registry/commit/431fb20144185fcb510fb817d753f7a1f75e295c))


### Features

* guard postalcode of box number address ([#970](https://github.com/informatievlaanderen/address-registry/issues/970)) ([9e1d9bd](https://github.com/informatievlaanderen/address-registry/commit/9e1d9bdda476bb1aa0b905e2dc8b84d36c0e89d3))

## [3.79.2](https://github.com/informatievlaanderen/address-registry/compare/v3.79.1...v3.79.2) (2023-03-13)


### Bug Fixes

* rename AddressMatchCollection to AddressMatchOsloCollection ([3b6ceae](https://github.com/informatievlaanderen/address-registry/commit/3b6ceae4730cb2ef295bfc4ba579dfbc28e4eab4))

## [3.79.1](https://github.com/informatievlaanderen/address-registry/compare/v3.79.0...v3.79.1) (2023-03-13)


### Bug Fixes

* addressmatch v2 produces jsonld ([ad413ec](https://github.com/informatievlaanderen/address-registry/commit/ad413ec0d5a6b075883adfba25de129a1b2e6c49))
* AddressWasRemovedBecauseStreetNameWasRemoved tags ([8aed11a](https://github.com/informatievlaanderen/address-registry/commit/8aed11a7896a9f108c7ea02251fc8722cf48b58a))

# [3.79.0](https://github.com/informatievlaanderen/address-registry/compare/v3.78.0...v3.79.0) (2023-03-13)


### Features

* add address match v2 ([853b367](https://github.com/informatievlaanderen/address-registry/commit/853b36791a6545e1a1f8ff7cf551a29db301cbcb))
* add addressmatch to oslo ([b4c2d28](https://github.com/informatievlaanderen/address-registry/commit/b4c2d28f1a902dfd9dd7c4794e660f3c6d8f587d))
* copy addressmatch tests & refactor namespaces ([9beb4ab](https://github.com/informatievlaanderen/address-registry/commit/9beb4ab75c5edd6e0378cdec38b0eca12cab0a36))

# [3.78.0](https://github.com/informatievlaanderen/address-registry/compare/v3.77.3...v3.78.0) (2023-03-08)


### Features

* add v2 address match to legacy ([10d29c0](https://github.com/informatievlaanderen/address-registry/commit/10d29c0fff348a09ad304fc2411ba37155a044a1))

## [3.77.3](https://github.com/informatievlaanderen/address-registry/compare/v3.77.2...v3.77.3) (2023-03-07)


### Bug Fixes

* temporary enable addressposition events in BU sync ([93c28ad](https://github.com/informatievlaanderen/address-registry/commit/93c28ad5c5602cfdd5e3a189dfef28c14b41cec9))

## [3.77.2](https://github.com/informatievlaanderen/address-registry/compare/v3.77.1...v3.77.2) (2023-03-07)


### Bug Fixes

* make producer reliable ([cfed4ff](https://github.com/informatievlaanderen/address-registry/commit/cfed4ff82cda9976f5dd3ac717dd23573766a3e8))

## [3.77.1](https://github.com/informatievlaanderen/address-registry/compare/v3.77.0...v3.77.1) (2023-03-06)


### Bug Fixes

* typo in migration ([7210895](https://github.com/informatievlaanderen/address-registry/commit/7210895cbad39cdf18c7831fb472c6c3d1499f96))

# [3.77.0](https://github.com/informatievlaanderen/address-registry/compare/v3.76.2...v3.77.0) (2023-03-06)


### Features

* create addresslistview and use in addresslist ([08c1e62](https://github.com/informatievlaanderen/address-registry/commit/08c1e6274ceef6b66e311ae630bf73b422bc900c))
* remove address because streetname was removed ([072d893](https://github.com/informatievlaanderen/address-registry/commit/072d893ace2416eb38ebdab7c8e22dd96ffa0e74))

## [3.76.2](https://github.com/informatievlaanderen/address-registry/compare/v3.76.1...v3.76.2) (2023-03-06)


### Performance Improvements

* add streetnamelatest syndication index ([6fcbc9d](https://github.com/informatievlaanderen/address-registry/commit/6fcbc9de1c6f274aee6d03f03af26eb81b39fb7a))

## [3.76.1](https://github.com/informatievlaanderen/address-registry/compare/v3.76.0...v3.76.1) (2023-03-01)


### Bug Fixes

* response examples ([eae4164](https://github.com/informatievlaanderen/address-registry/commit/eae4164656226cb883b0bbd1ddb9d892de445b8d))

# [3.76.0](https://github.com/informatievlaanderen/address-registry/compare/v3.75.0...v3.76.0) (2023-03-01)


### Bug Fixes

* bump mediatr ([#948](https://github.com/informatievlaanderen/address-registry/issues/948)) ([92f228f](https://github.com/informatievlaanderen/address-registry/commit/92f228ff8ef009bc3c827d9203287f7c4edcf27b))
* change volledigadresnaam to volledigadres ([73f7021](https://github.com/informatievlaanderen/address-registry/commit/73f70218fa7335ab54943d267ed38ad6f01f3efd))
* no merge group for ksql ([487d0ad](https://github.com/informatievlaanderen/address-registry/commit/487d0ada9e29ba08e784389d433cf21e26b955ca))
* ordering ksql files ([c062711](https://github.com/informatievlaanderen/address-registry/commit/c062711d99cbbce2ee255af5e28abfbb532583b3))


### Features

* add v2 examples ([46ddcb8](https://github.com/informatievlaanderen/address-registry/commit/46ddcb8112a45e7b5dedb497a108dab619d46258))

# [3.75.0](https://github.com/informatievlaanderen/address-registry/compare/v3.74.2...v3.75.0) (2023-02-27)


### Bug Fixes

* add debug code ([c00b547](https://github.com/informatievlaanderen/address-registry/commit/c00b547fcdc390506e289cd6f0abb841f1c400d0))
* add semicolon ([e1903f1](https://github.com/informatievlaanderen/address-registry/commit/e1903f1fc4ed019bef60adaf80fa85c1700ffcf7))
* bump grar common to 18.1.1 ([2abb161](https://github.com/informatievlaanderen/address-registry/commit/2abb161dc9224bff3ad9b30e3718109d36a73b7b))
* don't escape backticks ([ca55b19](https://github.com/informatievlaanderen/address-registry/commit/ca55b19a1e593abc3856ca31aac1cf47d7e68214))
* don't escape single quotes ([a1448b4](https://github.com/informatievlaanderen/address-registry/commit/a1448b48fda2a9a19d5c6d6c9edfd184d6175ecc))
* don't escape single quotes ([dd1ae33](https://github.com/informatievlaanderen/address-registry/commit/dd1ae332a809ada9af54be66a45c9798abe82a07))
* don't replace backticks ([a8b5dea](https://github.com/informatievlaanderen/address-registry/commit/a8b5dea14729d5048ecfef4e90a812f567c4d89b))
* echo contents of $FILE ([22b449b](https://github.com/informatievlaanderen/address-registry/commit/22b449b5d4bb5ee1a0a516e5395597f4908f5c63))
* escape double quotes in curl ([cbdfa79](https://github.com/informatievlaanderen/address-registry/commit/cbdfa79628d2afc14699ee5f4596439a72b18f86))
* exec only first file ([f0d25c8](https://github.com/informatievlaanderen/address-registry/commit/f0d25c8c0049a12559ca13550608dbed070132a0))
* found extraneous semicolon ([370f3e6](https://github.com/informatievlaanderen/address-registry/commit/370f3e637b05821b901c3691e84a458c4fb07886))
* ksql on single line ([be279f2](https://github.com/informatievlaanderen/address-registry/commit/be279f2be76aec1fd12b72c5ca733c0f3ba3854c))
* loop through ksql files ([381e667](https://github.com/informatievlaanderen/address-registry/commit/381e6679528b7bb13341c78a7a177b03ec69cfc1))
* loop thru .ksql files ([8455452](https://github.com/informatievlaanderen/address-registry/commit/845545258c8269d5083d74c33ee3ee98b9967354))
* move semicolon at end of loop ([2db60cc](https://github.com/informatievlaanderen/address-registry/commit/2db60cc687ff43abbe914c9e0ab932741dce7842))
* pushd/popd ([268a54f](https://github.com/informatievlaanderen/address-registry/commit/268a54fdf5f9bb8a357064664efdf6cf1f8277d1))
* remove debug statements ([edc83f4](https://github.com/informatievlaanderen/address-registry/commit/edc83f4b907138efe776996e281ff555ae8a2a9c))
* remove semicolon at end of loop ([3a76550](https://github.com/informatievlaanderen/address-registry/commit/3a7655044d621bd2851419756f755cf5a260b6ba))
* remove semicolon from 01~.ksql ([9fa2e3e](https://github.com/informatievlaanderen/address-registry/commit/9fa2e3eeaf774af2e768085409319d939073fe26))
* rename kqsl to ksql ([7fa7c1f](https://github.com/informatievlaanderen/address-registry/commit/7fa7c1f93f180142d24a5daaeabb1c377f566107))
* replace single quotes, double quotes & backticks in ksql files ([5a2b64c](https://github.com/informatievlaanderen/address-registry/commit/5a2b64cdcf91dbb48e475b5c0056e0928e297bf3))
* unescaped double quote ([3476a69](https://github.com/informatievlaanderen/address-registry/commit/3476a6930ed9f008fb030580297ef83351a00cd2))


### Features

* consume streetname homonym additions ([a0916e7](https://github.com/informatievlaanderen/address-registry/commit/a0916e7e26059b6e00288decdb40d43c937ed246))

## [3.74.2](https://github.com/informatievlaanderen/address-registry/compare/v3.74.1...v3.74.2) (2023-02-22)


### Bug Fixes

* deploy ci bump ([041a8e5](https://github.com/informatievlaanderen/address-registry/commit/041a8e5941c5b86515f66f6512d1c96038ca2d65))

## [3.74.1](https://github.com/informatievlaanderen/address-registry/compare/v3.74.0...v3.74.1) (2023-02-22)


### Bug Fixes

* bump to test new release ([4818e6f](https://github.com/informatievlaanderen/address-registry/commit/4818e6f2176b4dcae876cb05a25e32dc7b695a2e))

# [3.74.0](https://github.com/informatievlaanderen/address-registry/compare/v3.73.9...v3.74.0) (2023-02-22)


### Bug Fixes

* add response status code ([cdc9781](https://github.com/informatievlaanderen/address-registry/commit/cdc9781e8b6a5b3a83c295cd559c9d68dea3a52d))
* deploy ksql statements ([28f4383](https://github.com/informatievlaanderen/address-registry/commit/28f4383766c177d4918fa45bf6a2a0c207a2be72))
* silence curl ([822cbf0](https://github.com/informatievlaanderen/address-registry/commit/822cbf03ea0e0eb67e0c23d7ef978bf1e43f1200))
* use new ci workflow ([69d41c4](https://github.com/informatievlaanderen/address-registry/commit/69d41c4c0211d1b22df2b20d9c65acef9c5fff02))


### Features

* reject streetname underlying addresses ([a52c291](https://github.com/informatievlaanderen/address-registry/commit/a52c29131def86a148cbbea3fa145804e3362956))

## [3.73.9](https://github.com/informatievlaanderen/address-registry/compare/v3.73.8...v3.73.9) (2023-02-21)


### Bug Fixes

* consumer should commit if message is already processed ([1a57808](https://github.com/informatievlaanderen/address-registry/commit/1a57808733e89e2cacdd7662040feb40c349fe18))
* produce box numbers ([4f5d033](https://github.com/informatievlaanderen/address-registry/commit/4f5d03337b0516f84d3f251977fbe00520ff2839))
* produce oslo snapshots for AddressHouseNumberWasCorrectedV2 boxnumbers ([6fe9d4e](https://github.com/informatievlaanderen/address-registry/commit/6fe9d4eecc199cd52a8d3f1ebebb8ee13e2cc58f))

## [3.73.8](https://github.com/informatievlaanderen/address-registry/compare/v3.73.7...v3.73.8) (2023-02-17)


### Bug Fixes

* remove not required registrations ([a2abf96](https://github.com/informatievlaanderen/address-registry/commit/a2abf960b3d75f2bf07452492c0f26331635c1fe))
* splitup legacy commandhandler registrations ([d2bdc9a](https://github.com/informatievlaanderen/address-registry/commit/d2bdc9a58bb1b9e0273d554e26e78a1235ec59c6))

## [3.73.7](https://github.com/informatievlaanderen/address-registry/compare/v3.73.6...v3.73.7) (2023-02-16)


### Bug Fixes

* lock nodatime to 3.1.6 ([ae5855e](https://github.com/informatievlaanderen/address-registry/commit/ae5855efb3c372f7979574fda96c743a35f9e3c6))

## [3.73.6](https://github.com/informatievlaanderen/address-registry/compare/v3.73.5...v3.73.6) (2023-02-16)


### Bug Fixes

* add ProposeAddressSqsRequestFactory registration ([d22c5b5](https://github.com/informatievlaanderen/address-registry/commit/d22c5b54bc20101da9e4c6a870c14bdcf86cf119))

## [3.73.5](https://github.com/informatievlaanderen/address-registry/compare/v3.73.4...v3.73.5) (2023-02-15)

## [3.73.4](https://github.com/informatievlaanderen/address-registry/compare/v3.73.3...v3.73.4) (2023-02-15)


### Bug Fixes

* propose address lambda handler idempotency ([25bc3bf](https://github.com/informatievlaanderen/address-registry/commit/25bc3bf28dfbce0dcaba0c45a401740e1f379141))
* use merge queue ([87075c2](https://github.com/informatievlaanderen/address-registry/commit/87075c21bbd048e81e4b48b187ce632b148d7f85))

## [3.73.3](https://github.com/informatievlaanderen/address-registry/compare/v3.73.2...v3.73.3) (2023-02-14)


### Bug Fixes

* snapshot registration in lambda ([1c99672](https://github.com/informatievlaanderen/address-registry/commit/1c99672fdd844de6c6fc2b952ce8340a58d0d4e4))

## [3.73.2](https://github.com/informatievlaanderen/address-registry/compare/v3.73.1...v3.73.2) (2023-02-13)


### Bug Fixes

* add acmidm provenance impl ([adbb817](https://github.com/informatievlaanderen/address-registry/commit/adbb817f5f62eedb61a7c207fb7b652ad7bcbe80))
* clean up csproj ([3970c52](https://github.com/informatievlaanderen/address-registry/commit/3970c52618d095ed54a63a6295efc8b749e3c418))
* event jsondata should contain provenance ([30ab13d](https://github.com/informatievlaanderen/address-registry/commit/30ab13ddf81d18ed76402df47ccfb38a145fb869))
* fix sonar security warning ([edbc64c](https://github.com/informatievlaanderen/address-registry/commit/edbc64c246cb678338bbdce56d6b9223b2d3e72f))
* fix sonar security warnings ([16ab234](https://github.com/informatievlaanderen/address-registry/commit/16ab23404cf6c444c9cf02a0602d6973fb5bf7ad))
* place ksql in correct file ([fb1b7e7](https://github.com/informatievlaanderen/address-registry/commit/fb1b7e7dc5ea882928ae86cb723f693dbf4aec4c))
* remove addresspersistentlocalid from proposeaddress identityfields ([4078334](https://github.com/informatievlaanderen/address-registry/commit/40783342886239d27d4a1d3900db96eca1979d92))
* snapshot module registration ([e3c5b25](https://github.com/informatievlaanderen/address-registry/commit/e3c5b25c26130e230e0f62822c23859bbc3b63fe))
* volledigAdres in legacy and oslo apis ([017960c](https://github.com/informatievlaanderen/address-registry/commit/017960c53d704f457d59255efe412e6397d83e7a))

## [3.73.1](https://github.com/informatievlaanderen/address-registry/compare/v3.73.0...v3.73.1) (2023-01-31)

# [3.73.0](https://github.com/informatievlaanderen/address-registry/compare/v3.72.0...v3.73.0) (2023-01-25)


### Features

* add commandhandling streetname consumer ([61ce0e7](https://github.com/informatievlaanderen/address-registry/commit/61ce0e711c31e6dbf2e9c90b3762aa44bac01009))

# [3.72.0](https://github.com/informatievlaanderen/address-registry/compare/v3.71.0...v3.72.0) (2023-01-18)


### Bug Fixes

* catch parentInvalidStatusException in correctRegularization lambda ([28d506f](https://github.com/informatievlaanderen/address-registry/commit/28d506fb3d10dd77b707e86797f0b5588cc86112))
* default iscomplete in extract GAWR-4251 ([948583b](https://github.com/informatievlaanderen/address-registry/commit/948583bdd7855d65d53652e3501cb80912620b30))
* temporarily disable acm/idm ([55bfcc5](https://github.com/informatievlaanderen/address-registry/commit/55bfcc5c6cc2de611130562f548d1cbdbb8756f0))


### Features

* add acm/idm ([b2a932d](https://github.com/informatievlaanderen/address-registry/commit/b2a932dd53babc3ee3e6337403ed4bda2944af89))
* add projections for StreetNameNamesWereCorrected ([154ea12](https://github.com/informatievlaanderen/address-registry/commit/154ea12a46f1079d2c9bbdaa0a5107264cd2ebac))
* add StreetNameNamesWereCorrected to domain ([feff03d](https://github.com/informatievlaanderen/address-registry/commit/feff03d5b1ee2b571630133c31cc09901684319c))

# [3.71.0](https://github.com/informatievlaanderen/address-registry/compare/v3.70.3...v3.71.0) (2023-01-12)


### Bug Fixes

* rename deregularization to deregulation ([99cc5b6](https://github.com/informatievlaanderen/address-registry/commit/99cc5b67800eee200c5879344210a637c1ed6306))


### Features

* add correct address deregularization ([add55a5](https://github.com/informatievlaanderen/address-registry/commit/add55a56cf8c9b7d700a146ef876d141bb2f7fdb))
* add validation to correct regularization when parent is proposed ([1b89c26](https://github.com/informatievlaanderen/address-registry/commit/1b89c26a022239ef98409b43e029e3dcd58d2cfa))
* correct regularized address ([fdfda2d](https://github.com/informatievlaanderen/address-registry/commit/fdfda2d245d5e8b8529085f016b85b375ba7ca33))
* update Be.Vlaanderen.Basisregisters.Api to 19.0.1 ([92acdd1](https://github.com/informatievlaanderen/address-registry/commit/92acdd1bb6d22c96be1692bc185059d6cefcc957))

## [3.70.3](https://github.com/informatievlaanderen/address-registry/compare/v3.70.2...v3.70.3) (2023-01-09)


### Bug Fixes

* add backoffice projections to cd ([3aa62d2](https://github.com/informatievlaanderen/address-registry/commit/3aa62d2646482e4bc3bb339df5b4694496c1bda7))

## [3.70.2](https://github.com/informatievlaanderen/address-registry/compare/v3.70.1...v3.70.2) (2023-01-06)


### Bug Fixes

* add index to projections syndication AddressParcelLinksExtract ([652a798](https://github.com/informatievlaanderen/address-registry/commit/652a798af9c961145983efe0220b95b467bbf05b))

## [3.70.1](https://github.com/informatievlaanderen/address-registry/compare/v3.70.0...v3.70.1) (2023-01-05)


### Bug Fixes

* typo in add index migration script ([071ebd9](https://github.com/informatievlaanderen/address-registry/commit/071ebd92011ae1af1ed401e8fc4200f96c9628ad))

# [3.70.0](https://github.com/informatievlaanderen/address-registry/compare/v3.69.0...v3.70.0) (2023-01-04)


### Features

* expand index on syndication item AddressBuildingUnitLinksExtract ([375a58b](https://github.com/informatievlaanderen/address-registry/commit/375a58ba7175a7426586590ebc6118e3b083bed5))

# [3.69.0](https://github.com/informatievlaanderen/address-registry/compare/v3.68.2...v3.69.0) (2022-12-29)


### Features

* add backoffice projections ([e9bf0b3](https://github.com/informatievlaanderen/address-registry/commit/e9bf0b342005050fd5d9259b20ef896fe9fe2f4c))

## [3.68.2](https://github.com/informatievlaanderen/address-registry/compare/v3.68.1...v3.68.2) (2022-12-12)

## [3.68.1](https://github.com/informatievlaanderen/address-registry/compare/v3.68.0...v3.68.1) (2022-12-06)


### Bug Fixes

* publish api.legacy to nuget ([98df2de](https://github.com/informatievlaanderen/address-registry/commit/98df2ded54f175a06361a164ef339094cd9ffdda))

# [3.68.0](https://github.com/informatievlaanderen/address-registry/compare/v3.67.2...v3.68.0) (2022-12-06)


### Bug Fixes

* remove old workflows ([66863ba](https://github.com/informatievlaanderen/address-registry/commit/66863ba06f5750c79770a7377893e778935b97e5))
* remove release-new.yml ([29d9095](https://github.com/informatievlaanderen/address-registry/commit/29d9095199339f60a67d23ac02fcdf0a418dddcb))
* use correct token in release pipeline ([b3da85f](https://github.com/informatievlaanderen/address-registry/commit/b3da85fdbe8b9ee58763253a9bcc818abac1d3e0))


### Features

* require address position ([8203b1e](https://github.com/informatievlaanderen/address-registry/commit/8203b1ea8f0f0eb5585026928c6bedae54726671))

## [3.67.2](https://github.com/informatievlaanderen/address-registry/compare/v3.67.1...v3.67.2) (2022-12-04)


### Bug Fixes

* fix lambda source folder ([5fcc901](https://github.com/informatievlaanderen/address-registry/commit/5fcc90119578bce465ca28695b80dcf5e43bce66))

## [3.67.1](https://github.com/informatievlaanderen/address-registry/compare/v3.67.0...v3.67.1) (2022-12-04)


### Bug Fixes

* change workflow ([d5f8030](https://github.com/informatievlaanderen/address-registry/commit/d5f8030ab02ed69c722745e5230b173cbbe90cd4))
* small inconsistencies ([76f6d83](https://github.com/informatievlaanderen/address-registry/commit/76f6d83348d6f4337c2be7f0d863a5d7db932c0d))

# [3.67.0](https://github.com/informatievlaanderen/address-registry/compare/v3.66.3...v3.67.0) (2022-12-02)


### Bug Fixes

* add k6 script ([ce96663](https://github.com/informatievlaanderen/address-registry/commit/ce96663567f3b041a3ebc2182e3dd08a54c49e95))
* change url's to production ([391e1aa](https://github.com/informatievlaanderen/address-registry/commit/391e1aa5c76f85036b2bf96fb823a82c47a12a90))
* deduplicate LoggingModule.cs ([0a65aee](https://github.com/informatievlaanderen/address-registry/commit/0a65aee240bb1b8cf5b11e15b3bd4f36f877d48d))
* update adresmatch performance baseline ([a732e39](https://github.com/informatievlaanderen/address-registry/commit/a732e39e95853335b3f023bcf7ec479922c69363))


### Features

* correct boxnumber inconsistent housenumber ([af4c856](https://github.com/informatievlaanderen/address-registry/commit/af4c856c56f010547489c93494fd0e72fd4489d9))
* correct boxnumber inconsistent postalcode ([4159e30](https://github.com/informatievlaanderen/address-registry/commit/4159e30dd769512c47aae087f5f09a3fcdfd5904))

## [3.66.3](https://github.com/informatievlaanderen/address-registry/compare/v3.66.2...v3.66.3) (2022-11-03)


### Bug Fixes

* update ci & test branch protection ([932b2e6](https://github.com/informatievlaanderen/address-registry/commit/932b2e62cd9969943cf0df171afc70d770bfb28b))

## [3.66.2](https://github.com/informatievlaanderen/address-registry/compare/v3.66.1...v3.66.2) (2022-11-02)


### Bug Fixes

* style to bump build ([eb86325](https://github.com/informatievlaanderen/address-registry/commit/eb8632508f83b3c11ce5acfca2341f9f850d9118))

## [3.66.1](https://github.com/informatievlaanderen/address-registry/compare/v3.66.0...v3.66.1) (2022-11-02)


### Bug Fixes

* add nuget to dependabot ([684faf1](https://github.com/informatievlaanderen/address-registry/commit/684faf1b6fe07dd23ba4ebe112b61c074b93d92e))
* correct ef registration of bosacontext ([dc4274c](https://github.com/informatievlaanderen/address-registry/commit/dc4274c00dd7cc800b76175b9066566f452ced61))
* use VBR_SONAR_TOKEN ([55c6aae](https://github.com/informatievlaanderen/address-registry/commit/55c6aaefe2824411d86af002913b9a699a8094b5))

# [3.66.0](https://github.com/informatievlaanderen/address-registry/compare/v3.65.0...v3.66.0) (2022-10-31)


### Bug Fixes

* enable pr's & coverage ([27be1fc](https://github.com/informatievlaanderen/address-registry/commit/27be1fc0574a83f091ef5c806f4df571365b1f61))


### Features

* add streetnameid filter on V1 Legacy & Oslo ([b601e03](https://github.com/informatievlaanderen/address-registry/commit/b601e030fa496c19274c81473ce4c9250c593921))
* add streetnameid filter on V2 Legacy & Oslo ([7c3ab0f](https://github.com/informatievlaanderen/address-registry/commit/7c3ab0fca2c81f5ff6165254047ab3b64f0ceb00))

# [3.65.0](https://github.com/informatievlaanderen/address-registry/compare/v3.64.5...v3.65.0) (2022-10-28)


### Features

* update sqs ([e325ad0](https://github.com/informatievlaanderen/address-registry/commit/e325ad0cb412af7a0b75b95c8e3b1fcd67337f8b))

## [3.64.5](https://github.com/informatievlaanderen/address-registry/compare/v3.64.4...v3.64.5) (2022-10-27)


### Bug Fixes

* add braces ([ceadf6f](https://github.com/informatievlaanderen/address-registry/commit/ceadf6fdbdaf1b6bee88fe54471a22c571bf35e8))
* add braces ([bff2948](https://github.com/informatievlaanderen/address-registry/commit/bff2948259824290ecc977d16582e37e78a899a1))

## [3.64.4](https://github.com/informatievlaanderen/address-registry/compare/v3.64.3...v3.64.4) (2022-10-26)


### Bug Fixes

* add streetname status validation for address changes ([bb607a0](https://github.com/informatievlaanderen/address-registry/commit/bb607a0cd2d63982cf2235b9a0cd2b46a6a2fce5))

## [3.64.3](https://github.com/informatievlaanderen/address-registry/compare/v3.64.2...v3.64.3) (2022-10-25)


### Bug Fixes

* assertion of failing addressmatch test ([d15f900](https://github.com/informatievlaanderen/address-registry/commit/d15f900e6aff47a717441ea4b65b34be359d5e44))
* gawr-3884 AddressMatch StripStreetName ([e99649d](https://github.com/informatievlaanderen/address-registry/commit/e99649d98104e7bab305d3dba920497ceecdedf3))
* remove  from wms views ([b09e0e2](https://github.com/informatievlaanderen/address-registry/commit/b09e0e291398267ba9972fdc5198b569996ea88b))
* remove unneeded registration in snapshot oslo ([dcc91d9](https://github.com/informatievlaanderen/address-registry/commit/dcc91d9adc7e9c1d2a464338ca79ec0182bf1847))

## [3.64.2](https://github.com/informatievlaanderen/address-registry/compare/v3.64.1...v3.64.2) (2022-10-21)


### Bug Fixes

* prevent half consumption of kafka GAWR-3859 GAWR-3879 ([1f2cb42](https://github.com/informatievlaanderen/address-registry/commit/1f2cb422fd453ba0fa4b5ff4482d9177922b55cf))

## [3.64.1](https://github.com/informatievlaanderen/address-registry/compare/v3.64.0...v3.64.1) (2022-10-19)


### Bug Fixes

* address topic key produce oslo snapshot ([bfeeea6](https://github.com/informatievlaanderen/address-registry/commit/bfeeea63c8dfa1e02d6e1cdec2ede46c8dd96218))

# [3.64.0](https://github.com/informatievlaanderen/address-registry/compare/v3.63.4...v3.64.0) (2022-10-19)


### Bug Fixes

* actions ([2a16f1d](https://github.com/informatievlaanderen/address-registry/commit/2a16f1d2e5b432b3a6e478dadb4223fea466c086))
* add parameter to KafkaProducerOptions ctor ([03c68a8](https://github.com/informatievlaanderen/address-registry/commit/03c68a86015b6c34b2ef2bf48db79a82b49b737b))


### Features

* add ldes ([6c71632](https://github.com/informatievlaanderen/address-registry/commit/6c71632aebc34bcf9b1b1572f29044047ae81176))

## [3.63.4](https://github.com/informatievlaanderen/address-registry/compare/v3.63.3...v3.63.4) (2022-10-19)


### Bug Fixes

* validation error fixes ([bf21df9](https://github.com/informatievlaanderen/address-registry/commit/bf21df954484970e6ed0a499157cc4c10b3876d0))

## [3.63.3](https://github.com/informatievlaanderen/address-registry/compare/v3.63.2...v3.63.3) (2022-10-17)


### Bug Fixes

* update sqs packages ([7672710](https://github.com/informatievlaanderen/address-registry/commit/7672710932d3dec0425ea63c34f6a6b0ca4f702d))

## [3.63.2](https://github.com/informatievlaanderen/address-registry/compare/v3.63.1...v3.63.2) (2022-10-17)


### Bug Fixes

* register idempotent command handler ([a33663e](https://github.com/informatievlaanderen/address-registry/commit/a33663e87b219f967ac080eedeef181a2c49d1c4))

## [3.63.1](https://github.com/informatievlaanderen/address-registry/compare/v3.63.0...v3.63.1) (2022-10-17)


### Bug Fixes

* add SqsAddressCorrectBoxNumberRequest to messagehandler ([fa83213](https://github.com/informatievlaanderen/address-registry/commit/fa83213bfffe8e9ed377f26e725123852614f116))
* base lambdahandler innermapdomainexception ([fa183e8](https://github.com/informatievlaanderen/address-registry/commit/fa183e840de86558b57534107d79913cbc07b06d))
* use basisregisters.sqs.lamba ([58f7e7c](https://github.com/informatievlaanderen/address-registry/commit/58f7e7ccc318707d5de8d3a31fb2b8b50430c940))

# [3.63.0](https://github.com/informatievlaanderen/address-registry/compare/v3.62.2...v3.63.0) (2022-10-14)


### Bug Fixes

* removed address error message and code ([0f0c815](https://github.com/informatievlaanderen/address-registry/commit/0f0c81577884dd747b1a50a6dbd719302f8ce45f))
* restore build.fsx ([b3d0582](https://github.com/informatievlaanderen/address-registry/commit/b3d05825ccc2cc29fdeafc8383cc322327cca889))
* typo in reject handler ([95cdb33](https://github.com/informatievlaanderen/address-registry/commit/95cdb33a09128ecb3d113a188cb6b543a325c02d))
* use Be.Vlaanderen.Basisregisters.Sqs ([1c82098](https://github.com/informatievlaanderen/address-registry/commit/1c820980bfc2f801e82bc215d2cd88655d8e5642))
* use Build.Pipeline 6.0.5 ([a6863e3](https://github.com/informatievlaanderen/address-registry/commit/a6863e390f54410659a869bb2e92e9f7e2eb7fec))


### Features

* throw when sqs request is not mapped to lambda request ([ee7f89e](https://github.com/informatievlaanderen/address-registry/commit/ee7f89e850e792c35ae65c90b258a06b7cc49d7a))

## [3.62.2](https://github.com/informatievlaanderen/address-registry/compare/v3.62.1...v3.62.2) (2022-10-13)


### Bug Fixes

* register IMunicipalies in lambda ([cb45e84](https://github.com/informatievlaanderen/address-registry/commit/cb45e84f2cee82bf39285a30aa214bba8d13ea39))

## [3.62.1](https://github.com/informatievlaanderen/address-registry/compare/v3.62.0...v3.62.1) (2022-10-13)


### Bug Fixes

* use ILifetimescope instead of IContainer ([a71278a](https://github.com/informatievlaanderen/address-registry/commit/a71278a5f4e4e9424d44082edafd484bb8a1fb32))

# [3.62.0](https://github.com/informatievlaanderen/address-registry/compare/v3.61.0...v3.62.0) (2022-10-12)


### Features

* streetname status corrections ([2dfcacd](https://github.com/informatievlaanderen/address-registry/commit/2dfcacda5789d11e0731e39f86de1e87a31d35ac))

# [3.61.0](https://github.com/informatievlaanderen/address-registry/compare/v3.60.0...v3.61.0) (2022-10-12)


### Bug Fixes

* add boxnumber validation for empty ([8542af5](https://github.com/informatievlaanderen/address-registry/commit/8542af5a2c3ee02d87b2c6a2f41ada324db63f61))


### Features

* reject streetname in domain ([a546bd0](https://github.com/informatievlaanderen/address-registry/commit/a546bd038d635cde559db3f90d1b4c79635097f5))

# [3.60.0](https://github.com/informatievlaanderen/address-registry/compare/v3.59.4...v3.60.0) (2022-10-11)


### Features

* add validation on streetname status before letting through corrections ([0ffbeeb](https://github.com/informatievlaanderen/address-registry/commit/0ffbeeb8eb46251fc8ac9548faaa519633a79c6b))

## [3.59.4](https://github.com/informatievlaanderen/address-registry/compare/v3.59.3...v3.59.4) (2022-10-11)


### Bug Fixes

* parent check for corrections ([51ecd35](https://github.com/informatievlaanderen/address-registry/commit/51ecd35a402fc4d8d717c1237eb19aa172def30c))
* propose status check correctretirement ([3e3e5fa](https://github.com/informatievlaanderen/address-registry/commit/3e3e5fafaabab1916488e87b4b04ee31062cdfba))
* use generated build.yml ([742bd50](https://github.com/informatievlaanderen/address-registry/commit/742bd5029b7a583891a35750e1d6cc7586f8d086))

## [3.59.3](https://github.com/informatievlaanderen/address-registry/compare/v3.59.2...v3.59.3) (2022-10-11)


### Bug Fixes

* compare house numbers with letters only ([8b8250f](https://github.com/informatievlaanderen/address-registry/commit/8b8250f96694952112eb72c407362a927298d5f7))

## [3.59.2](https://github.com/informatievlaanderen/address-registry/compare/v3.59.1...v3.59.2) (2022-10-11)

## [3.59.1](https://github.com/informatievlaanderen/address-registry/compare/v3.59.0...v3.59.1) (2022-10-11)


### Bug Fixes

* sonar issues ([a3bb9f2](https://github.com/informatievlaanderen/address-registry/commit/a3bb9f2eb931ba81d5e3d17d1ccefecaf6231956))

# [3.59.0](https://github.com/informatievlaanderen/address-registry/compare/v3.58.2...v3.59.0) (2022-10-10)


### Features

* wms v2 ([9af46cf](https://github.com/informatievlaanderen/address-registry/commit/9af46cfb57080c5f7e8bd8c8524258a66c7e2209))

## [3.58.2](https://github.com/informatievlaanderen/address-registry/compare/v3.58.1...v3.58.2) (2022-10-10)


### Bug Fixes

* add duplicate address validation for correct reject/retire ([1c482cd](https://github.com/informatievlaanderen/address-registry/commit/1c482cdfbaf66d81d440942c85979474c66e5364))
* parent address status validations before correct reject/retire ([9b54848](https://github.com/informatievlaanderen/address-registry/commit/9b54848367ca8e0f2a52685b47b917790dc1a8d8))

## [3.58.1](https://github.com/informatievlaanderen/address-registry/compare/v3.58.0...v3.58.1) (2022-10-10)


### Bug Fixes

* schema of wms views ([de1beaf](https://github.com/informatievlaanderen/address-registry/commit/de1beaff1058936d938a0f09e3c1b15601d51027))

# [3.58.0](https://github.com/informatievlaanderen/address-registry/compare/v3.57.2...v3.58.0) (2022-10-07)


### Bug Fixes

* build wms refactoring ([2ed5c53](https://github.com/informatievlaanderen/address-registry/commit/2ed5c53ac2b90d3fe5c4f42be118c1ab36db4ac5))


### Features

* refactor wms ([f4acdab](https://github.com/informatievlaanderen/address-registry/commit/f4acdab1a1c4603e15070058d8c699dbb9788fa2))

## [3.57.2](https://github.com/informatievlaanderen/address-registry/compare/v3.57.1...v3.57.2) (2022-10-05)


### Bug Fixes

* clean up unneeded execution ([3d314ee](https://github.com/informatievlaanderen/address-registry/commit/3d314ee2b7a814e65ae10258fa4f69d3107ef2cf))

## [3.57.1](https://github.com/informatievlaanderen/address-registry/compare/v3.57.0...v3.57.1) (2022-10-05)


### Bug Fixes

* error code when correcting address rejection ([895aeca](https://github.com/informatievlaanderen/address-registry/commit/895aeca5d46777d85dbc45c88b9566c8eecf7038))

# [3.57.0](https://github.com/informatievlaanderen/address-registry/compare/v3.56.3...v3.57.0) (2022-10-05)


### Features

* add parent addresss status validation for deregulate box number address ([6cd774c](https://github.com/informatievlaanderen/address-registry/commit/6cd774ce98feb763e62dd32d3082ba9e10a2fd7b))

## [3.56.3](https://github.com/informatievlaanderen/address-registry/compare/v3.56.2...v3.56.3) (2022-10-04)


### Bug Fixes

* style to trigger build ([960d53b](https://github.com/informatievlaanderen/address-registry/commit/960d53b50acbe210d34796e37321ec55ef0c9728))

## [3.56.2](https://github.com/informatievlaanderen/address-registry/compare/v3.56.1...v3.56.2) (2022-10-04)


### Bug Fixes

* correct address retirement route ([0a5ecfa](https://github.com/informatievlaanderen/address-registry/commit/0a5ecfac6da58f09d4a1f3505fff1c843a776ddf))

## [3.56.1](https://github.com/informatievlaanderen/address-registry/compare/v3.56.0...v3.56.1) (2022-10-04)


### Bug Fixes

* event documentation ([6d9ce80](https://github.com/informatievlaanderen/address-registry/commit/6d9ce80a10b62652fdcd7aa8353c3d4fa3c2c92d))
* sqs handler registration ([cb3e5e8](https://github.com/informatievlaanderen/address-registry/commit/cb3e5e84891b278383c52fdcb74aba255561d4a2))

# [3.56.0](https://github.com/informatievlaanderen/address-registry/compare/v3.55.0...v3.56.0) (2022-10-03)


### Features

* correct address retirement ([a3ffd47](https://github.com/informatievlaanderen/address-registry/commit/a3ffd470121b2025586eaba884559fd063914296))

# [3.55.0](https://github.com/informatievlaanderen/address-registry/compare/v3.54.0...v3.55.0) (2022-10-03)


### Features

* add correct reject ([1436410](https://github.com/informatievlaanderen/address-registry/commit/143641090ced207aa502efb4290e20620efc118f))
* correct addres approval backoffice and projection ([bb2d51b](https://github.com/informatievlaanderen/address-registry/commit/bb2d51b064a0b0bcc323b40d2cbe7ae792ee1b79))

# [3.54.0](https://github.com/informatievlaanderen/address-registry/compare/v3.53.0...v3.54.0) (2022-10-03)


### Features

* deregulated address becomes current ([c5c64bd](https://github.com/informatievlaanderen/address-registry/commit/c5c64bdd066e6fd2dc0adc94efcbfb1c19e5eb53))

# [3.53.0](https://github.com/informatievlaanderen/address-registry/compare/v3.52.0...v3.53.0) (2022-09-30)


### Features

* correct address approval domain ([4fa8372](https://github.com/informatievlaanderen/address-registry/commit/4fa837258a8b09c42486bbc08594c33ac00ea7b3))

# [3.52.0](https://github.com/informatievlaanderen/address-registry/compare/v3.51.1...v3.52.0) (2022-09-28)


### Features

* add correct boxnumber ([0ba67b4](https://github.com/informatievlaanderen/address-registry/commit/0ba67b437b96c6dd8d66412c4bd42087768e9f6c))
* handle AggregateIdIsNotFoundException in API ([b35add9](https://github.com/informatievlaanderen/address-registry/commit/b35add98ae90afe020594cea73fb834922f0904d))

## [3.51.1](https://github.com/informatievlaanderen/address-registry/compare/v3.51.0...v3.51.1) (2022-09-26)


### Bug Fixes

* remove datacontract attr from some requests ([f62b908](https://github.com/informatievlaanderen/address-registry/commit/f62b908333fc09284cbb755482f3961c4ff9eddb))

# [3.51.0](https://github.com/informatievlaanderen/address-registry/compare/v3.50.4...v3.51.0) (2022-09-26)


### Bug Fixes

* lambda csproj for multiple publish output files problem ([e41c47f](https://github.com/informatievlaanderen/address-registry/commit/e41c47f0a0282a6ea1c97c36acfa9045e9b399a8))
* remove elemetns from csproj file ([6349afc](https://github.com/informatievlaanderen/address-registry/commit/6349afc919455fd9e12f3dc4a2392f2ccde8b65d))


### Features

* refactor to async ([4efef58](https://github.com/informatievlaanderen/address-registry/commit/4efef58bbfa9f400c86ee481fe9590e6fa3feca3))

## [3.50.3](https://github.com/informatievlaanderen/address-registry/compare/v3.50.2...v3.50.3) (2022-09-21)


### Bug Fixes

* remove duplicate unique validation in correct housenumber GAWR-3706 ([811f1bf](https://github.com/informatievlaanderen/address-registry/commit/811f1bf261d20558224474d1a21ef467dcd3b3a4))
* set property for correct postal validation GAWR-3705 ([c58c4aa](https://github.com/informatievlaanderen/address-registry/commit/c58c4aa4075dde2c8038f27096685a6bfbae7c16))

## [3.50.2](https://github.com/informatievlaanderen/address-registry/compare/v3.50.1...v3.50.2) (2022-09-20)


### Bug Fixes

* fix sonar issues ([1974042](https://github.com/informatievlaanderen/address-registry/commit/1974042b2394979f55ab6dd5fba94e46da4daa9b))

## [3.50.1](https://github.com/informatievlaanderen/address-registry/compare/v3.50.0...v3.50.1) (2022-09-20)


### Bug Fixes

* migration AddressBoxNumberSyndicationHelperÂ ([e70aef7](https://github.com/informatievlaanderen/address-registry/commit/e70aef7399773420ba5e9d13a05c86cd4f77c3fd))

# [3.50.0](https://github.com/informatievlaanderen/address-registry/compare/v3.49.0...v3.50.0) (2022-09-20)


### Features

* add AddressBoxNumberSyndication ef migration ([d9190bc](https://github.com/informatievlaanderen/address-registry/commit/d9190bc4c104847613160f49b126082d73336f61))

# [3.49.0](https://github.com/informatievlaanderen/address-registry/compare/v3.48.1...v3.49.0) (2022-09-16)


### Features

* correct address house number backoffice ([62978f1](https://github.com/informatievlaanderen/address-registry/commit/62978f1a3d19f8d56f9ce03188f13e867c6685fe))
* correct address house number projections ([84ff850](https://github.com/informatievlaanderen/address-registry/commit/84ff850063315814f978280cee6ec3190b27ecea))

## [3.48.1](https://github.com/informatievlaanderen/address-registry/compare/v3.48.0...v3.48.1) (2022-09-16)


### Bug Fixes

* include box numbers in postal code changed and corrected events ([a3a5faa](https://github.com/informatievlaanderen/address-registry/commit/a3a5faad89931b438b52548aa636bedf432ad2fe))

# [3.48.0](https://github.com/informatievlaanderen/address-registry/compare/v3.47.2...v3.48.0) (2022-09-16)


### Features

* correct address house number domain ([7a13c3d](https://github.com/informatievlaanderen/address-registry/commit/7a13c3db9961f8ce7a4926de220c4f6365ee6204))

## [3.47.2](https://github.com/informatievlaanderen/address-registry/compare/v3.47.1...v3.47.2) (2022-09-16)


### Performance Improvements

* recreate wfs view with indexed view ([6a03717](https://github.com/informatievlaanderen/address-registry/commit/6a037173f561446ba78d1e21455f3f88ff4fce4a))

## [3.47.1](https://github.com/informatievlaanderen/address-registry/compare/v3.47.0...v3.47.1) (2022-09-15)


### Performance Improvements

* add wfs index ([1b4a1e7](https://github.com/informatievlaanderen/address-registry/commit/1b4a1e715252a8d501701e776bc3c0f4a32464b4))

# [3.47.0](https://github.com/informatievlaanderen/address-registry/compare/v3.46.0...v3.47.0) (2022-09-14)


### Bug Fixes

* register StreetNameConsumer EF configuration ([a2473d6](https://github.com/informatievlaanderen/address-registry/commit/a2473d618ce0366daca3852f89470d969010b662))


### Features

* address postalcode was corrected projections ([a4e3814](https://github.com/informatievlaanderen/address-registry/commit/a4e3814a40b184612a4b2c69f6e35217c0456fa9))
* correct address postal code backoffice + lambda ([5f3e9ef](https://github.com/informatievlaanderen/address-registry/commit/5f3e9efb65de2fd4f8ea784ecd59104e763ba835))

# [3.46.0](https://github.com/informatievlaanderen/address-registry/compare/v3.45.0...v3.46.0) (2022-09-14)


### Features

* address postalcode was corrected ([9493bcc](https://github.com/informatievlaanderen/address-registry/commit/9493bcc5dea93bea9708c47fd1590ee0690e974d))

# [3.45.0](https://github.com/informatievlaanderen/address-registry/compare/v3.44.0...v3.45.0) (2022-09-13)


### Bug Fixes

* add distributed lock ([722f4d0](https://github.com/informatievlaanderen/address-registry/commit/722f4d0fe7757618f891b60817824443fa5c2e59))


### Features

* change address postalcode backoffice ([139c019](https://github.com/informatievlaanderen/address-registry/commit/139c019e81f6e531b6be8f6adbbf146d0c2320bb))

# [3.44.0](https://github.com/informatievlaanderen/address-registry/compare/v3.43.0...v3.44.0) (2022-09-13)


### Bug Fixes

* trigger build ([b9cb5ce](https://github.com/informatievlaanderen/address-registry/commit/b9cb5ce97c750ab898877bcaf622ad0a9847b4f6))


### Features

* cascade change address postalcode to box numbers ([4fb5fbf](https://github.com/informatievlaanderen/address-registry/commit/4fb5fbf823d75d7802634abe233b9cc8c58067a3))
* change address postalcode projections + kafka ([e107daa](https://github.com/informatievlaanderen/address-registry/commit/e107daa074e7e7ef43160cfc198833e28f1e32fe))

# [3.43.0](https://github.com/informatievlaanderen/address-registry/compare/v3.42.1...v3.43.0) (2022-09-13)


### Bug Fixes

* pack & containerize streetname consumer ([dd9c677](https://github.com/informatievlaanderen/address-registry/commit/dd9c677df11b8821148f13a718b782ea4b936c99))


### Features

* change address postal code domain ([c304d86](https://github.com/informatievlaanderen/address-registry/commit/c304d8609b8b1419931fa200ece2ea728a4291cf))

## [3.42.1](https://github.com/informatievlaanderen/address-registry/compare/v3.42.0...v3.42.1) (2022-09-13)


### Bug Fixes

* streetname consumer github workflows ([b49ec49](https://github.com/informatievlaanderen/address-registry/commit/b49ec498b44ebfa97f12b305ce8772fb77795101))

# [3.42.0](https://github.com/informatievlaanderen/address-registry/compare/v3.41.0...v3.42.0) (2022-09-13)


### Features

* add remove address backoffice and sqs lambda ([2d2c0b5](https://github.com/informatievlaanderen/address-registry/commit/2d2c0b5a55abe00c605880e6aff806aa4efe9382))

# [3.41.0](https://github.com/informatievlaanderen/address-registry/compare/v3.40.0...v3.41.0) (2022-09-12)


### Features

* add remove boxnumber addresses + kafka ([dc3b95f](https://github.com/informatievlaanderen/address-registry/commit/dc3b95fe1fa86c70f2965876b3da3533a27b09ff))

# [3.40.0](https://github.com/informatievlaanderen/address-registry/compare/v3.39.3...v3.40.0) (2022-09-12)


### Features

* add remove address projections ([9cb75cb](https://github.com/informatievlaanderen/address-registry/commit/9cb75cbc70dfef2195d3de06357435ce76a360d5))
* remove address ([ba52a90](https://github.com/informatievlaanderen/address-registry/commit/ba52a900e5e1061337e73b46a87b91397f2ac5f5))

## [3.39.3](https://github.com/informatievlaanderen/address-registry/compare/v3.39.2...v3.39.3) (2022-09-12)

## [3.39.2](https://github.com/informatievlaanderen/address-registry/compare/v3.39.1...v3.39.2) (2022-09-12)

## [3.39.1](https://github.com/informatievlaanderen/address-registry/compare/v3.39.0...v3.39.1) (2022-09-08)


### Bug Fixes

* change error reason `PositionRequired` GAWR-3649 ([ac19376](https://github.com/informatievlaanderen/address-registry/commit/ac19376a36a750f4ec108ffc222900ee73e46385))

# [3.39.0](https://github.com/informatievlaanderen/address-registry/compare/v3.38.13...v3.39.0) (2022-09-08)


### Features

* add streetname read projections ([a5e51ee](https://github.com/informatievlaanderen/address-registry/commit/a5e51eedce685e2e3b09336eb231a0529c5bb367))

## [3.38.13](https://github.com/informatievlaanderen/address-registry/compare/v3.38.12...v3.38.13) (2022-09-06)


### Bug Fixes

* change workflow ([7945642](https://github.com/informatievlaanderen/address-registry/commit/7945642d30bb8d2644f88f539386fe32d30fe165))

## [3.38.12](https://github.com/informatievlaanderen/address-registry/compare/v3.38.11...v3.38.12) (2022-09-05)


### Bug Fixes

* fix format strings ([9cb8831](https://github.com/informatievlaanderen/address-registry/commit/9cb8831eeff2caa830a526d8efffc15a4fb199e1))

## [3.38.11](https://github.com/informatievlaanderen/address-registry/compare/v3.38.10...v3.38.11) (2022-09-05)


### Bug Fixes

* 500 error on empty addressposition ([821e8f4](https://github.com/informatievlaanderen/address-registry/commit/821e8f46bd82a94d2f248bb34617867aa04d8122))

## [3.38.10](https://github.com/informatievlaanderen/address-registry/compare/v3.38.9...v3.38.10) (2022-09-02)


### Bug Fixes

* correct syndication of streetname GAWR-3637 ([a9840c9](https://github.com/informatievlaanderen/address-registry/commit/a9840c9ad29b023e18a134e5500bd60c81a97e09))


### Performance Improvements

* improve extract v2 performance address ([4ca8ff6](https://github.com/informatievlaanderen/address-registry/commit/4ca8ff65f5593df138471e8588c8fdea5d631a98))

## [3.38.9](https://github.com/informatievlaanderen/address-registry/compare/v3.38.8...v3.38.9) (2022-09-01)


### Bug Fixes

* event order when rejecting or retiring house number vs box number addresses ([c2b4fee](https://github.com/informatievlaanderen/address-registry/commit/c2b4fee5cccf88073f6b786e77ba724d8a1b80d1))

## [3.38.8](https://github.com/informatievlaanderen/address-registry/compare/v3.38.7...v3.38.8) (2022-09-01)


### Bug Fixes

* add missing ConsumerMunicipality connection strings ([f755b07](https://github.com/informatievlaanderen/address-registry/commit/f755b07c7df6332bc93166aa6ed87d62e91c3bfb))

## [3.38.7](https://github.com/informatievlaanderen/address-registry/compare/v3.38.6...v3.38.7) (2022-08-31)


### Bug Fixes

* documentation on request and event properties ([d11f31a](https://github.com/informatievlaanderen/address-registry/commit/d11f31ad31e64753e7bef5961af723dc6c97eb40))
* positionGeometryMethod on propose address should be optional ([dcd6475](https://github.com/informatievlaanderen/address-registry/commit/dcd6475c0b1ef5fa178b50f855e745495703b5f2))

## [3.38.6](https://github.com/informatievlaanderen/address-registry/compare/v3.38.5...v3.38.6) (2022-08-31)


### Bug Fixes

* register missing MunicipalityConsumerModule ([5d677e3](https://github.com/informatievlaanderen/address-registry/commit/5d677e35f59990fccaf7af0006cc76f7227c37be))

## [3.38.5](https://github.com/informatievlaanderen/address-registry/compare/v3.38.4...v3.38.5) (2022-08-31)


### Bug Fixes

* event description for property GeometrySpecification ([4a4c6f1](https://github.com/informatievlaanderen/address-registry/commit/4a4c6f11fa6b417762ddad73b245997d2e3c58ce))

## [3.38.4](https://github.com/informatievlaanderen/address-registry/compare/v3.38.3...v3.38.4) (2022-08-30)


### Bug Fixes

* return etag for address legacy/oslo GAWR-3608 ([0f84405](https://github.com/informatievlaanderen/address-registry/commit/0f84405643cd2f6c7c995a2ca6b1d3c0f8a0dde2))

## [3.38.3](https://github.com/informatievlaanderen/address-registry/compare/v3.38.2...v3.38.3) (2022-08-30)

## [3.38.2](https://github.com/informatievlaanderen/address-registry/compare/v3.38.1...v3.38.2) (2022-08-29)


### Bug Fixes

* register ConsumerMunicipalityContext ([ec35f11](https://github.com/informatievlaanderen/address-registry/commit/ec35f1138b59bd0f912796ae5b58b366d7f0fdd9))

## [3.38.1](https://github.com/informatievlaanderen/address-registry/compare/v3.38.0...v3.38.1) (2022-08-29)


### Bug Fixes

* change syndication IDs from GUID to string ([3a7064b](https://github.com/informatievlaanderen/address-registry/commit/3a7064ba8109a35801b55cef040d78b2d5bea279))

# [3.38.0](https://github.com/informatievlaanderen/address-registry/compare/v3.37.4...v3.38.0) (2022-08-26)


### Bug Fixes

* tests with mystery character ([617cfce](https://github.com/informatievlaanderen/address-registry/commit/617cfcec1a517767ec949c68606ef5bfbbe8a6fc))
* validation bug gawr-3587 ([9cdbc8b](https://github.com/informatievlaanderen/address-registry/commit/9cdbc8b1d31d65c68c95a0cedd71746914c1edec))


### Features

* default position for proposeAddress ([7062c30](https://github.com/informatievlaanderen/address-registry/commit/7062c303fa3581691730cc53a9272dd5b206595a))
* validate parent house number status on approve bus number ([0d4b9b8](https://github.com/informatievlaanderen/address-registry/commit/0d4b9b8ef497ee1776721894425f901f879dba97))

## [3.37.4](https://github.com/informatievlaanderen/address-registry/compare/v3.37.3...v3.37.4) (2022-08-24)


### Bug Fixes

* change errorcodes ([082ad5c](https://github.com/informatievlaanderen/address-registry/commit/082ad5c66f6268a345a566dad8707c8835eb66ee))

## [3.37.3](https://github.com/informatievlaanderen/address-registry/compare/v3.37.2...v3.37.3) (2022-08-24)


### Bug Fixes

* snapshot ([cf07220](https://github.com/informatievlaanderen/address-registry/commit/cf07220c6700f4fd1e6db9710418697b96d6556d))

## [3.37.2](https://github.com/informatievlaanderen/address-registry/compare/v3.37.1...v3.37.2) (2022-08-23)


### Bug Fixes

* validate on duplicate persistent local id ([c3377ff](https://github.com/informatievlaanderen/address-registry/commit/c3377ff07bbdcefceac768f5987169ce718aef81))

## [3.37.1](https://github.com/informatievlaanderen/address-registry/compare/v3.37.0...v3.37.1) (2022-08-23)

# [3.37.0](https://github.com/informatievlaanderen/address-registry/compare/v3.36.0...v3.37.0) (2022-08-23)


### Features

* add correctAddressPosition ([6b8a4d0](https://github.com/informatievlaanderen/address-registry/commit/6b8a4d063ef4ec5f8a62336dd28cbdeb5008b6c5))

# [3.36.0](https://github.com/informatievlaanderen/address-registry/compare/v3.35.0...v3.36.0) (2022-08-23)


### Features

* add validation error tests for addressChangePosition ([6d4bd19](https://github.com/informatievlaanderen/address-registry/commit/6d4bd1923446b74f86d2dea0072dd9e9f2f8f323))

# [3.35.0](https://github.com/informatievlaanderen/address-registry/compare/v3.34.1...v3.35.0) (2022-08-22)


### Features

* add change address position ([f28ebb8](https://github.com/informatievlaanderen/address-registry/commit/f28ebb81425dbc6f862646045c57d53e6c65980a))

## [3.34.1](https://github.com/informatievlaanderen/address-registry/compare/v3.34.0...v3.34.1) (2022-08-19)

# [3.34.0](https://github.com/informatievlaanderen/address-registry/compare/v3.33.2...v3.34.0) (2022-08-19)


### Features

* propose address validation on position ([b8c8bc6](https://github.com/informatievlaanderen/address-registry/commit/b8c8bc6e9e1bb12174e78afb3e2e5c1d6bfc766c))

## [3.33.2](https://github.com/informatievlaanderen/address-registry/compare/v3.33.1...v3.33.2) (2022-08-17)


### Bug Fixes

* add events to migration producer ([bb74348](https://github.com/informatievlaanderen/address-registry/commit/bb743482e331116bc5fa5ea26d9ed3eb810e40ee))

## [3.33.1](https://github.com/informatievlaanderen/address-registry/compare/v3.33.0...v3.33.1) (2022-08-17)

# [3.33.0](https://github.com/informatievlaanderen/address-registry/compare/v3.32.0...v3.33.0) (2022-08-17)


### Features

* propose address validation on position specification ([9127d54](https://github.com/informatievlaanderen/address-registry/commit/9127d5416454d0ccd150d6aff98eefd8c136242a))

# [3.32.0](https://github.com/informatievlaanderen/address-registry/compare/v3.31.1...v3.32.0) (2022-08-17)


### Features

* propose address validation on position geometry method ([85ff945](https://github.com/informatievlaanderen/address-registry/commit/85ff945f23a66c2aaa5c050b9d3241e4500647e2))

## [3.31.1](https://github.com/informatievlaanderen/address-registry/compare/v3.31.0...v3.31.1) (2022-08-16)

# [3.31.0](https://github.com/informatievlaanderen/address-registry/compare/v3.30.7...v3.31.0) (2022-08-16)


### Features

* reject proposed bus number address on retire house number address ([423f481](https://github.com/informatievlaanderen/address-registry/commit/423f4817e4ccc792392e424c37100f46259581e3))

## [3.30.7](https://github.com/informatievlaanderen/address-registry/compare/v3.30.6...v3.30.7) (2022-08-12)


### Bug Fixes

* rename event descriptions for AddressWasRegularized and AddressWasDeregulated ([7022ced](https://github.com/informatievlaanderen/address-registry/commit/7022ced7370a1a530dac3d072b26e2d19c45d9d5))

## [3.30.6](https://github.com/informatievlaanderen/address-registry/compare/v3.30.5...v3.30.6) (2022-08-12)


### Bug Fixes

* add ticketingService baseUrl in appsettings ([09c7c95](https://github.com/informatievlaanderen/address-registry/commit/09c7c95b5c616c3ef44655d08024bc7ae3f74b18))
* change validation message in reject address ([6b6dc31](https://github.com/informatievlaanderen/address-registry/commit/6b6dc31b6ceb35a273f160c6e5eccbf6588ab36d))

## [3.30.5](https://github.com/informatievlaanderen/address-registry/compare/v3.30.4...v3.30.5) (2022-08-12)


### Bug Fixes

* review 672 status validation ([6259c36](https://github.com/informatievlaanderen/address-registry/commit/6259c3666da0fd2fb4e2feed33356c38c9995be0))

## [3.30.4](https://github.com/informatievlaanderen/address-registry/compare/v3.30.3...v3.30.4) (2022-08-11)


### Bug Fixes

* Update legacy list projection on address was regularized or deregulated ([84fdd36](https://github.com/informatievlaanderen/address-registry/commit/84fdd36cb426e665ed21412a117fafe7da620bc4))

## [3.30.3](https://github.com/informatievlaanderen/address-registry/compare/v3.30.2...v3.30.3) (2022-08-11)


### Bug Fixes

* review PRs 667 and 668 ([32d39c3](https://github.com/informatievlaanderen/address-registry/commit/32d39c363e054b37657c73509f216f3d929e8b23))

## [3.30.2](https://github.com/informatievlaanderen/address-registry/compare/v3.30.1...v3.30.2) (2022-08-11)


### Bug Fixes

* Map address status Rejected ([75f67ff](https://github.com/informatievlaanderen/address-registry/commit/75f67ff57326a1e0d868a7140f1beb5d09c0d543))
* review ([0bbd7f4](https://github.com/informatievlaanderen/address-registry/commit/0bbd7f4a42f9fd0af338b6e7399cbd7fc6abd0c0))

## [3.30.1](https://github.com/informatievlaanderen/address-registry/compare/v3.30.0...v3.30.1) (2022-08-10)


### Bug Fixes

* regularize and deregulate address error messages ([815c5b8](https://github.com/informatievlaanderen/address-registry/commit/815c5b83c2aed88cd10a46182b7bc15b94c2b8d9))

# [3.30.0](https://github.com/informatievlaanderen/address-registry/compare/v3.29.1...v3.30.0) (2022-08-09)


### Bug Fixes

* StreetNameWasRejected Kafka consumption ([6974bdc](https://github.com/informatievlaanderen/address-registry/commit/6974bdcf64eb5ebf77952dc47e8935bc39c51332))


### Features

* sqs refactor ([54cafbe](https://github.com/informatievlaanderen/address-registry/commit/54cafbece3d37890e1ed2dd5448cb9eb03529d16))

## [3.29.1](https://github.com/informatievlaanderen/address-registry/compare/v3.29.0...v3.29.1) (2022-08-09)


### Bug Fixes

* broken test ([e14fee8](https://github.com/informatievlaanderen/address-registry/commit/e14fee8d8b3d17a0b203b298963c152673622444))
* change error code to AdresGehistoreerdOfInGebruik ([3c414e7](https://github.com/informatievlaanderen/address-registry/commit/3c414e7a6ff9b77037468af285e3fb635ba0a5c6))
* success response from 204 to 202 ([be507cc](https://github.com/informatievlaanderen/address-registry/commit/be507ccd98a825acc418e63418884ca736f6752c))

# [3.29.0](https://github.com/informatievlaanderen/address-registry/compare/v3.28.0...v3.29.0) (2022-08-03)


### Features

* complete automatic retire and reject of bus number addresses with extra event ([1fe11dc](https://github.com/informatievlaanderen/address-registry/commit/1fe11dc580625a24f3cf8ee116b63a5dfbd6c7b8))

# [3.28.0](https://github.com/informatievlaanderen/address-registry/compare/v3.27.0...v3.28.0) (2022-08-03)


### Features

* move if-match header validation to ifmatchheadervalidator ([eb6916a](https://github.com/informatievlaanderen/address-registry/commit/eb6916a5365c8fd397d26354c1a2ad9a2f9b25ee))
* retire address ([6356e8d](https://github.com/informatievlaanderen/address-registry/commit/6356e8d8135c38a36058c579cf1f7e9f602759c4))

# [3.27.0](https://github.com/informatievlaanderen/address-registry/compare/v3.26.0...v3.27.0) (2022-08-01)


### Features

* regularize address ([96d0460](https://github.com/informatievlaanderen/address-registry/commit/96d046022f3f2814023f2a9cb65c7620b274b75b))

# [3.26.0](https://github.com/informatievlaanderen/address-registry/compare/v3.25.0...v3.26.0) (2022-07-29)


### Features

* deregulate an address ([87c7577](https://github.com/informatievlaanderen/address-registry/commit/87c757799c7cb484dc1317610279c57d2c068518))

# [3.25.0](https://github.com/informatievlaanderen/address-registry/compare/v3.24.5...v3.25.0) (2022-07-28)


### Bug Fixes

* static class & param name ([25b2fef](https://github.com/informatievlaanderen/address-registry/commit/25b2fefe89dd0ec54a5f301035a6f060c68d8275))


### Features

* reject address ([12fe650](https://github.com/informatievlaanderen/address-registry/commit/12fe650c9fba0225d5aa087b636597fb0d78dbb3))

## [3.24.5](https://github.com/informatievlaanderen/address-registry/compare/v3.24.4...v3.24.5) (2022-07-28)


### Bug Fixes

* braces style ([e4a3dd2](https://github.com/informatievlaanderen/address-registry/commit/e4a3dd27dff0284b21d25b9f652311241f5c1888))
* comment empty methods ([0c75a75](https://github.com/informatievlaanderen/address-registry/commit/0c75a759af2811869188582a9a9a0dc725c8c64d))
* correct format strings ([914fe1e](https://github.com/informatievlaanderen/address-registry/commit/914fe1e5c17cebfda1d3520fe91d99dccdfac8c1))
* default value on parameters ([68ee85d](https://github.com/informatievlaanderen/address-registry/commit/68ee85d41eddd7460ce5455b731263c4645ec8cd))
* don't throw general exceptions ([19cf4bc](https://github.com/informatievlaanderen/address-registry/commit/19cf4bcef5842dea7ce76415e4fa4948d249ac6e))
* extract nested ternary clauses ([e0ea22a](https://github.com/informatievlaanderen/address-registry/commit/e0ea22ad39993e7ee5f0b51ea2331ba425168193))
* fill empty methods ([c57bd2c](https://github.com/informatievlaanderen/address-registry/commit/c57bd2ceaf1721d66ecb67e445ca643b45e08c15))
* fix Serializable ([9f34249](https://github.com/informatievlaanderen/address-registry/commit/9f3424907d8455bcf414dac4ff17396108921779))
* ignore naming ([a40f3a5](https://github.com/informatievlaanderen/address-registry/commit/a40f3a5fea8fe911006d374a015f9f14d224291e))
* make classes static ([54c927b](https://github.com/informatievlaanderen/address-registry/commit/54c927be9704c2a03ca53696cfb35e91f5cc5a2b))
* match parameter names ([b34629d](https://github.com/informatievlaanderen/address-registry/commit/b34629ddcbf0533dcc795ef9e76b1d755458d9c1))
* merge collapsible if's ([b6f0f5f](https://github.com/informatievlaanderen/address-registry/commit/b6f0f5fcb48e25c27532ce0cb2451d784ab310aa))
* remove redundant case clauses ([ad3dd2c](https://github.com/informatievlaanderen/address-registry/commit/ad3dd2cb60c300115c09a70269f68459faf3c9ee))
* remove unused members ([e60b91c](https://github.com/informatievlaanderen/address-registry/commit/e60b91c5d49f46a058a91d5c701addc6e2fc6f8a))
* remove unused variables ([049e40e](https://github.com/informatievlaanderen/address-registry/commit/049e40e3b9bc0faba109ce6d3cc537bae9ac37f4))
* return empty array instead of null ([b6f25e1](https://github.com/informatievlaanderen/address-registry/commit/b6f25e1385f4eee18565a48337002d3982d8df52))
* simplify loops with linq ([f092d44](https://github.com/informatievlaanderen/address-registry/commit/f092d4423e009f439f60aaeabcfd4ce251c3eefb))
* unused parameters ([ca4fdc2](https://github.com/informatievlaanderen/address-registry/commit/ca4fdc24252a4ec1b6b82d746a107122afc5286e))
* visibility of fields ([7dc7339](https://github.com/informatievlaanderen/address-registry/commit/7dc7339ad479a094788acd1b85c5714825f8e5fd))

## [3.24.4](https://github.com/informatievlaanderen/address-registry/compare/v3.24.3...v3.24.4) (2022-07-26)


### Bug Fixes

* fix sonar issues ([3633632](https://github.com/informatievlaanderen/address-registry/commit/363363243ea9f8564c0decdfeaa944b856aac1b5))

## [3.24.3](https://github.com/informatievlaanderen/address-registry/compare/v3.24.2...v3.24.3) (2022-07-26)


### Bug Fixes

* bump commandhandling dependency ([0a6c96c](https://github.com/informatievlaanderen/address-registry/commit/0a6c96cebe3056c38490a70ac7996ae06f33d943))

## [3.24.2](https://github.com/informatievlaanderen/address-registry/compare/v3.24.1...v3.24.2) (2022-07-26)


### Bug Fixes

* fix sonar issues ([bb4ebc6](https://github.com/informatievlaanderen/address-registry/commit/bb4ebc66904d6bb66231cd70302146fa03107318))

## [3.24.1](https://github.com/informatievlaanderen/address-registry/compare/v3.24.0...v3.24.1) (2022-07-13)

# [3.24.0](https://github.com/informatievlaanderen/address-registry/compare/v3.23.0...v3.24.0) (2022-07-12)


### Features

* finish consumer municipality ([f0dfebc](https://github.com/informatievlaanderen/address-registry/commit/f0dfebc796544a81705d1986924af0e6edcfac5c))

# [3.23.0](https://github.com/informatievlaanderen/address-registry/compare/v3.22.4...v3.23.0) (2022-07-12)


### Bug Fixes

* build ([8d21585](https://github.com/informatievlaanderen/address-registry/commit/8d21585f276119e3d0235491f8a12954e8793217))


### Features

* add mediator handlers + tests ([60daa98](https://github.com/informatievlaanderen/address-registry/commit/60daa982ebac3d438a129d56c0868a465142cfc7))

## [3.22.4](https://github.com/informatievlaanderen/address-registry/compare/v3.22.3...v3.22.4) (2022-07-11)


### Bug Fixes

* add snapshot migrator settings ([cae66ad](https://github.com/informatievlaanderen/address-registry/commit/cae66ad4e522aa285d35b53f3f020f1b9caeb5b4))

## [3.22.3](https://github.com/informatievlaanderen/address-registry/compare/v3.22.2...v3.22.3) (2022-07-01)

## [3.22.2](https://github.com/informatievlaanderen/address-registry/compare/v3.22.1...v3.22.2) (2022-06-30)


### Bug Fixes

* rename projection description ([e1a63a3](https://github.com/informatievlaanderen/address-registry/commit/e1a63a3e64fd94c9f15b06456fa78139f7ea56a2))
* rename projection description ([ccdaf11](https://github.com/informatievlaanderen/address-registry/commit/ccdaf112f3d6c1335fb73c5b22b97e41d267cd4c))

## [3.22.1](https://github.com/informatievlaanderen/address-registry/compare/v3.22.0...v3.22.1) (2022-06-30)


### Bug Fixes

* add LABEL to Dockerfile (for easier DataDog filtering) ([8de16cd](https://github.com/informatievlaanderen/address-registry/commit/8de16cd2d2c80b0c795265c237f7c8bafad06499))

# [3.22.0](https://github.com/informatievlaanderen/address-registry/compare/v3.21.1...v3.22.0) (2022-06-27)


### Features

* add postinfo municaplity validation ([b25627a](https://github.com/informatievlaanderen/address-registry/commit/b25627a0ee52b0a86a61e0691e79c700751883d0))

## [3.21.1](https://github.com/informatievlaanderen/address-registry/compare/v3.21.0...v3.21.1) (2022-06-23)


### Bug Fixes

* correct tpyo address approve validation ([6db624a](https://github.com/informatievlaanderen/address-registry/commit/6db624a3378a14896c6357017b742371f7b858c7))

# [3.21.0](https://github.com/informatievlaanderen/address-registry/compare/v3.20.0...v3.21.0) (2022-06-22)


### Features

* add snapshotting ([cb52321](https://github.com/informatievlaanderen/address-registry/commit/cb52321c7149ce94d819f16782183dc8b35412b2))

# [3.20.0](https://github.com/informatievlaanderen/address-registry/compare/v3.19.1...v3.20.0) (2022-06-21)


### Features

* approve validations ([c187937](https://github.com/informatievlaanderen/address-registry/commit/c1879370b0ba49b93bb79426fa24a3341bb5e285))

## [3.19.1](https://github.com/informatievlaanderen/address-registry/compare/v3.19.0...v3.19.1) (2022-06-16)


### Bug Fixes

* approve address request attributes ([486437d](https://github.com/informatievlaanderen/address-registry/commit/486437d07798458b4c0cd1c17a4023b80f90805b))

# [3.19.0](https://github.com/informatievlaanderen/address-registry/compare/v3.18.3...v3.19.0) (2022-06-08)


### Features

* add producer starting from migration ([737b0cc](https://github.com/informatievlaanderen/address-registry/commit/737b0ccb4acdba2f560d0853f7bd06ea45d298df))

## [3.18.3](https://github.com/informatievlaanderen/address-registry/compare/v3.18.2...v3.18.3) (2022-06-08)


### Bug Fixes

* don't reproduce streetname events ([080de14](https://github.com/informatievlaanderen/address-registry/commit/080de14afb14506105c3ec4117f2bed5c83fdcc7))

## [3.18.2](https://github.com/informatievlaanderen/address-registry/compare/v3.18.1...v3.18.2) (2022-06-02)

## [3.18.1](https://github.com/informatievlaanderen/address-registry/compare/v3.18.0...v3.18.1) (2022-05-30)


### Bug Fixes

* propose validation messages ([50df8a5](https://github.com/informatievlaanderen/address-registry/commit/50df8a5b2ab53634b066480c7a704da728442ab1))

# [3.18.0](https://github.com/informatievlaanderen/address-registry/compare/v3.17.0...v3.18.0) (2022-05-30)


### Features

* add producer to build ([1f84fab](https://github.com/informatievlaanderen/address-registry/commit/1f84fab292084649888e38b3620f607c11cd345a))

# [3.17.0](https://github.com/informatievlaanderen/address-registry/compare/v3.16.0...v3.17.0) (2022-05-30)


### Features

* add producer ([d573156](https://github.com/informatievlaanderen/address-registry/commit/d57315638850cc0f5f4c87a0d2211430b2dc6d17))

# [3.16.0](https://github.com/informatievlaanderen/address-registry/compare/v3.15.0...v3.16.0) (2022-05-24)


### Bug Fixes

* build ([ea6415e](https://github.com/informatievlaanderen/address-registry/commit/ea6415ec288db5b0af8c89f612cbdc1c745b9402))


### Features

* streetnameId validation tests ([3679a70](https://github.com/informatievlaanderen/address-registry/commit/3679a706a3eb3bb319532c30648d99ecbf306c5a))

# [3.15.0](https://github.com/informatievlaanderen/address-registry/compare/v3.14.2...v3.15.0) (2022-05-23)


### Bug Fixes

* correct clone streetnamepersistentlocalid in legacy sync ([5f7f0da](https://github.com/informatievlaanderen/address-registry/commit/5f7f0dad603728f040bd879f8d6792b67c5edf8c))


### Features

* add boxnumber & housenumber validations ([9bdca7b](https://github.com/informatievlaanderen/address-registry/commit/9bdca7ba80044376baad918696cbd5156a6ea359))

## [3.14.2](https://github.com/informatievlaanderen/address-registry/compare/v3.14.1...v3.14.2) (2022-05-23)


### Bug Fixes

* propose ([ccc6b01](https://github.com/informatievlaanderen/address-registry/commit/ccc6b01e2e2ee7b998fda4aa70c67996dca7966c))

## [3.14.1](https://github.com/informatievlaanderen/address-registry/compare/v3.14.0...v3.14.1) (2022-05-20)


### Bug Fixes

* fix legacy and oslo endpoints ([430a44e](https://github.com/informatievlaanderen/address-registry/commit/430a44e32ceccd770b2bc3f52ac783fd3a9ceffe))

# [3.14.0](https://github.com/informatievlaanderen/address-registry/compare/v3.13.0...v3.14.0) (2022-05-20)


### Features

* postInfoId validations ([5e92581](https://github.com/informatievlaanderen/address-registry/commit/5e92581f9c674ea3a09a9f868e4a877862430240))

# [3.13.0](https://github.com/informatievlaanderen/address-registry/compare/v3.12.0...v3.13.0) (2022-05-18)


### Features

* update projections for AddressWasApproved, make PostalCode nullable in legacy & wfs ([c200bcc](https://github.com/informatievlaanderen/address-registry/commit/c200bcc295d5f1ef5628d137e8146fb267cc1070))

# [3.12.0](https://github.com/informatievlaanderen/address-registry/compare/v3.11.1...v3.12.0) (2022-05-18)


### Features

* add approve controller + tests ([1d2b9d8](https://github.com/informatievlaanderen/address-registry/commit/1d2b9d80799f7cf1e597b483f51eefd5dae7ec1d))

## [3.11.1](https://github.com/informatievlaanderen/address-registry/compare/v3.11.0...v3.11.1) (2022-05-18)


### Bug Fixes

* correct address id after migration in sync ([564857c](https://github.com/informatievlaanderen/address-registry/commit/564857c47b5c76edc9c43ce3278bb1ee97a58575))
* correct docs event migration ([0a684ba](https://github.com/informatievlaanderen/address-registry/commit/0a684ba4bf124ceb44f7240c8c93c84b75c0b197))

# [3.11.0](https://github.com/informatievlaanderen/address-registry/compare/v3.10.2...v3.11.0) (2022-05-17)


### Features

* add approve command, event, aggregate, tests ([f038d17](https://github.com/informatievlaanderen/address-registry/commit/f038d17ef9eced72713f0704cecc8fe4efe4c2d0))

## [3.10.2](https://github.com/informatievlaanderen/address-registry/compare/v3.10.1...v3.10.2) (2022-05-17)


### Bug Fixes

* set event descriptions for AddressWasProposedV2 ([e4043f0](https://github.com/informatievlaanderen/address-registry/commit/e4043f0a9cb5a02ae75827fc3a59db406f1f0dce))

## [3.10.1](https://github.com/informatievlaanderen/address-registry/compare/v3.10.0...v3.10.1) (2022-05-16)


### Bug Fixes

* syndication projection set AddressId nullable in db ([d010702](https://github.com/informatievlaanderen/address-registry/commit/d01070230952a1311ebce507a214c2d02401ac15))

# [3.10.0](https://github.com/informatievlaanderen/address-registry/compare/v3.9.0...v3.10.0) (2022-05-16)


### Bug Fixes

* correct contract casing gawr-3055 gawr-3056 ([001b773](https://github.com/informatievlaanderen/address-registry/commit/001b77368025665620d6202b11a780584ee20673))
* gawr-3074 correct tags new events ([0cfca6c](https://github.com/informatievlaanderen/address-registry/commit/0cfca6cc388d17250269dd6364c7a043e1650a86))


### Features

* update projections after AddressWasProposedV2 ([f0ec457](https://github.com/informatievlaanderen/address-registry/commit/f0ec4578e16bfe882eaf25490daa1982271df04e))

# [3.9.0](https://github.com/informatievlaanderen/address-registry/compare/v3.8.13...v3.9.0) (2022-05-16)


### Bug Fixes

* build ([933a626](https://github.com/informatievlaanderen/address-registry/commit/933a626f12781a2863670d4435470be3021a4b61))


### Features

* add address propose integration test ([2a7044f](https://github.com/informatievlaanderen/address-registry/commit/2a7044f99f52837015124114ae9895828d0a835a))

## [3.8.13](https://github.com/informatievlaanderen/address-registry/compare/v3.8.12...v3.8.13) (2022-05-16)


### Bug Fixes

* remove alter column from previous projection ([ed8ce9e](https://github.com/informatievlaanderen/address-registry/commit/ed8ce9ea166702d292e72d84ce17fb475fb65f51))

## [3.8.12](https://github.com/informatievlaanderen/address-registry/compare/v3.8.11...v3.8.12) (2022-05-13)


### Bug Fixes

* upgrade kafka consumer ([146435c](https://github.com/informatievlaanderen/address-registry/commit/146435c70f18d7f7ff6bc3d891d955558f07b219))

## [3.8.11](https://github.com/informatievlaanderen/address-registry/compare/v3.8.10...v3.8.11) (2022-05-12)


### Bug Fixes

* finalize consumer ([cc1be79](https://github.com/informatievlaanderen/address-registry/commit/cc1be79b0850fe8c436fee4afe6f06b2ec15b481))

## [3.8.10](https://github.com/informatievlaanderen/address-registry/compare/v3.8.9...v3.8.10) (2022-05-12)


### Bug Fixes

* try using ([be3f1b8](https://github.com/informatievlaanderen/address-registry/commit/be3f1b89bce748c9995f2ea0d16605e29d4b0072))

## [3.8.9](https://github.com/informatievlaanderen/address-registry/compare/v3.8.8...v3.8.9) (2022-05-12)


### Bug Fixes

* added delay for projector to start up ([f0f0369](https://github.com/informatievlaanderen/address-registry/commit/f0f0369cdf7cf50537f53a897e0d2968802cfeaf))

## [3.8.8](https://github.com/informatievlaanderen/address-registry/compare/v3.8.7...v3.8.8) (2022-05-12)


### Bug Fixes

* add delay back into consumer ([2e69c22](https://github.com/informatievlaanderen/address-registry/commit/2e69c226487d364d4d46cb8366c94dee815d871a))

## [3.8.7](https://github.com/informatievlaanderen/address-registry/compare/v3.8.6...v3.8.7) (2022-05-12)


### Bug Fixes

* gawr-3040 add tag to migrate event ([ac78f87](https://github.com/informatievlaanderen/address-registry/commit/ac78f87258bfa0bc42adef90f3b494b519f643e2))

## [3.8.6](https://github.com/informatievlaanderen/address-registry/compare/v3.8.5...v3.8.6) (2022-05-12)


### Bug Fixes

* remove nameof > refactor broke it ([5dab0ca](https://github.com/informatievlaanderen/address-registry/commit/5dab0ca3f15b5218a168fbde1dafb28fb45d3879))

## [3.8.5](https://github.com/informatievlaanderen/address-registry/compare/v3.8.4...v3.8.5) (2022-05-11)


### Bug Fixes

* consumer add projector in own runner ([9cb84f3](https://github.com/informatievlaanderen/address-registry/commit/9cb84f32b5d9f3ba8aa70dd7b626035030ea764e))

## [3.8.4](https://github.com/informatievlaanderen/address-registry/compare/v3.8.3...v3.8.4) (2022-05-11)


### Bug Fixes

* copy consumer and add logging ([9dde6cc](https://github.com/informatievlaanderen/address-registry/commit/9dde6cc43c3fbe71ce694b71dcf61ba621bd7e38))

## [3.8.3](https://github.com/informatievlaanderen/address-registry/compare/v3.8.2...v3.8.3) (2022-05-11)


### Bug Fixes

* try different approach for consumer ([3a64fbd](https://github.com/informatievlaanderen/address-registry/commit/3a64fbdea9da03dbdb57646b6a1c3868d46bd657))

## [3.8.2](https://github.com/informatievlaanderen/address-registry/compare/v3.8.1...v3.8.2) (2022-05-11)


### Bug Fixes

* add logging ([16025ff](https://github.com/informatievlaanderen/address-registry/commit/16025ffe42bc043d39f061709b3c67101937ad56))
* build logging ([00d393b](https://github.com/informatievlaanderen/address-registry/commit/00d393bb80b436269eedac4cf9b80ec0056d2f06))

## [3.8.1](https://github.com/informatievlaanderen/address-registry/compare/v3.8.0...v3.8.1) (2022-05-10)


### Bug Fixes

* contract of propose request ([fa8a91b](https://github.com/informatievlaanderen/address-registry/commit/fa8a91be115871d6dbed6a408fafde45a4c6e2f8))

# [3.8.0](https://github.com/informatievlaanderen/address-registry/compare/v3.7.1...v3.8.0) (2022-05-10)


### Features

* add propose address ([132d5b6](https://github.com/informatievlaanderen/address-registry/commit/132d5b659369a2fdd2e0aa0ab3df22e1703c7c89))

## [3.7.1](https://github.com/informatievlaanderen/address-registry/compare/v3.7.0...v3.7.1) (2022-05-05)


### Bug Fixes

* make projector stay alive ([14a4a55](https://github.com/informatievlaanderen/address-registry/commit/14a4a555074e4c3c97b0ade0fb12fa1c8b70dd62))

# [3.7.0](https://github.com/informatievlaanderen/address-registry/compare/v3.6.3...v3.7.0) (2022-05-05)


### Bug Fixes

* fix test ([e178a6f](https://github.com/informatievlaanderen/address-registry/commit/e178a6f34b6d0acfdf0c37b10312c40077f329a8))


### Features

* add municipality consumer + tests for lastItem ([f982a82](https://github.com/informatievlaanderen/address-registry/commit/f982a82ceea98618c934081b07ac58c7fcd16a81))

## [3.6.3](https://github.com/informatievlaanderen/address-registry/compare/v3.6.2...v3.6.3) (2022-05-03)


### Bug Fixes

* skip unexpected cases in staging ([f21957a](https://github.com/informatievlaanderen/address-registry/commit/f21957ae75eba6b8538b6580f7f810d6f97dcd1d))
* test ([dd3e07f](https://github.com/informatievlaanderen/address-registry/commit/dd3e07fc40c52bc4b8def982bbeb1066a38ad371))

## [3.6.2](https://github.com/informatievlaanderen/address-registry/compare/v3.6.1...v3.6.2) (2022-05-02)


### Bug Fixes

* stuck migrator ([b0af684](https://github.com/informatievlaanderen/address-registry/commit/b0af6848c39f263a3ca2d0a1ab217437c21fef11))

## [3.6.1](https://github.com/informatievlaanderen/address-registry/compare/v3.6.0...v3.6.1) (2022-04-30)


### Performance Improvements

* resolve addresses per stream so it doesn't build up memory ([d8a61e7](https://github.com/informatievlaanderen/address-registry/commit/d8a61e7f5a41fd71c3919e0a492b00dd00582d98))

# [3.6.0](https://github.com/informatievlaanderen/address-registry/compare/v3.5.0...v3.6.0) (2022-04-29)


### Bug Fixes

* ambigous call after toggle in oslo list ([37b904a](https://github.com/informatievlaanderen/address-registry/commit/37b904a5f0316d5210be9a875ab6f76bd6e85efe))
* backoffice scope lifetime in migrator ([cd66dec](https://github.com/informatievlaanderen/address-registry/commit/cd66dec000b496a8cfae85e8a18ba176677b6fab))


### Features

* add toggles to projection api ([242b23e](https://github.com/informatievlaanderen/address-registry/commit/242b23ea2f5d2473724d5ecb492b2fb86e0eb453))

# [3.5.0](https://github.com/informatievlaanderen/address-registry/compare/v3.4.4...v3.5.0) (2022-04-29)


### Features

* add projection tests ([f1b0bbd](https://github.com/informatievlaanderen/address-registry/commit/f1b0bbdedaeae9ff5da1b41daa56d6822ad83b9a))
* add toggle projector v2 projections ([15a519d](https://github.com/informatievlaanderen/address-registry/commit/15a519dd48c79c672f5146ba3adbc0fc838527e0))

## [3.4.4](https://github.com/informatievlaanderen/address-registry/compare/v3.4.3...v3.4.4) (2022-04-29)


### Bug Fixes

* run sonar end when release version != none ([1f0f1de](https://github.com/informatievlaanderen/address-registry/commit/1f0f1de6a63578368187bd38b86e9ca63a344160))

## [3.4.3](https://github.com/informatievlaanderen/address-registry/compare/v3.4.2...v3.4.3) (2022-04-29)

## [3.4.2](https://github.com/informatievlaanderen/address-registry/compare/v3.4.1...v3.4.2) (2022-04-27)


### Bug Fixes

* add null check for boxnumber in migrated event ([00ece37](https://github.com/informatievlaanderen/address-registry/commit/00ece37582f63b7ad02cd94fb51b396eed261a22))

## [3.4.1](https://github.com/informatievlaanderen/address-registry/compare/v3.4.0...v3.4.1) (2022-04-27)


### Bug Fixes

* style to trigger build ([73ba0bb](https://github.com/informatievlaanderen/address-registry/commit/73ba0bb9ad150d75f43c3d6bae0cda9a75b5b8c8))

# [3.4.0](https://github.com/informatievlaanderen/address-registry/compare/v3.3.5...v3.4.0) (2022-04-27)


### Bug Fixes

* v2 projections build ([158cdfc](https://github.com/informatievlaanderen/address-registry/commit/158cdfcf8dc53e3ec8f9582734b6d2ea53234241))


### Features

* add v2 projections ([3e9b28a](https://github.com/informatievlaanderen/address-registry/commit/3e9b28a76a54cbeaf485c3427a537be6d80ebd27))

## [3.3.5](https://github.com/informatievlaanderen/address-registry/compare/v3.3.4...v3.3.5) (2022-04-27)


### Bug Fixes

* redirect sonar to /dev/null ([435ab01](https://github.com/informatievlaanderen/address-registry/commit/435ab0162c8955bb0b23cbe1b7e3254b945286a0))

## [3.3.4](https://github.com/informatievlaanderen/address-registry/compare/v3.3.3...v3.3.4) (2022-04-26)


### Bug Fixes

* use IMessage on events ([d344c70](https://github.com/informatievlaanderen/address-registry/commit/d344c70c22bd84a1976f4441a292b1b3ac8f8bda))

## [3.3.3](https://github.com/informatievlaanderen/address-registry/compare/v3.3.2...v3.3.3) (2022-04-26)


### Bug Fixes

* add migrator to build ([d066cf4](https://github.com/informatievlaanderen/address-registry/commit/d066cf45930915fda294eac9aa7f9dd4dd0f18ec))
* correct migrator backoffice csproj ([fb481e1](https://github.com/informatievlaanderen/address-registry/commit/fb481e18f0a5a17469cf0c4b530c591b572e1dd6))
* csproj migrator + backoffice naming ([1fcae14](https://github.com/informatievlaanderen/address-registry/commit/1fcae144ff229b0ef70f9705da5782d14825e86a))

## [3.3.2](https://github.com/informatievlaanderen/address-registry/compare/v3.3.1...v3.3.2) (2022-04-26)


### Bug Fixes

* add build backoffice ([99ddd29](https://github.com/informatievlaanderen/address-registry/commit/99ddd294c734ea91525605735e3956202ac16d35))
* solution ([f87efe9](https://github.com/informatievlaanderen/address-registry/commit/f87efe9cdae868b299beb4e23a012d2b14523188))

## [3.3.1](https://github.com/informatievlaanderen/address-registry/compare/v3.3.0...v3.3.1) (2022-04-25)


### Bug Fixes

* finish migrator ([0ed78b3](https://github.com/informatievlaanderen/address-registry/commit/0ed78b3e6d9813f40cf68080bfcf1c89eb016058))

# [3.3.0](https://github.com/informatievlaanderen/address-registry/compare/v3.2.5...v3.3.0) (2022-04-22)


### Bug Fixes

* build ([318b480](https://github.com/informatievlaanderen/address-registry/commit/318b4800ebda2bf63aff63744bcf9de4a495ceba))
* tests change to MigratedNisCode ([3f7ed22](https://github.com/informatievlaanderen/address-registry/commit/3f7ed22f5f7505bb120f1202f14708e614b2b2b6))


### Features

* add backoffice for migrator ([eab6d05](https://github.com/informatievlaanderen/address-registry/commit/eab6d05f13a5ab345ee980be72f1c76c47c83872))

## [3.2.5](https://github.com/informatievlaanderen/address-registry/compare/v3.2.4...v3.2.5) (2022-04-20)


### Bug Fixes

* recreate index wms GAWR-3003 ([4fe354d](https://github.com/informatievlaanderen/address-registry/commit/4fe354d30b7af9b83b7a2e755d5a30da9a2cddda))

## [3.2.4](https://github.com/informatievlaanderen/address-registry/compare/v3.2.3...v3.2.4) (2022-04-15)


### Bug Fixes

* break linkedfeed loop when cancellation is requested ([74a5401](https://github.com/informatievlaanderen/address-registry/commit/74a5401f8bcf929a55345b7502d48a981adb4085))
* style to trigger build ([1818ff4](https://github.com/informatievlaanderen/address-registry/commit/1818ff4bef302ce1378d420320b46d091954d77a))

## [3.2.3](https://github.com/informatievlaanderen/address-registry/compare/v3.2.2...v3.2.3) (2022-04-15)


### Bug Fixes

* fuzzymatch null checks ([0f03432](https://github.com/informatievlaanderen/address-registry/commit/0f03432747b30ac1a56f203b4abac5dc61c1dc35))

## [3.2.2](https://github.com/informatievlaanderen/address-registry/compare/v3.2.1...v3.2.2) (2022-04-13)


### Bug Fixes

* style to trigger build ([369a548](https://github.com/informatievlaanderen/address-registry/commit/369a5487fa3472f032a61a0ca16f876f9b7856e0))

## [3.2.1](https://github.com/informatievlaanderen/address-registry/compare/v3.2.0...v3.2.1) (2022-04-12)


### Performance Improvements

* bosa collect all postalcodes instead of one by one ([c753ba2](https://github.com/informatievlaanderen/address-registry/commit/c753ba2c5ea42029dd8ba38fc7f542f328b5894d))

# [3.2.0](https://github.com/informatievlaanderen/address-registry/compare/v3.1.7...v3.2.0) (2022-04-12)


### Bug Fixes

* run consumer projection + upgrade kafka package ([c1a893a](https://github.com/informatievlaanderen/address-registry/commit/c1a893a35c757ce7d6cac9314e3525d78bbe2943))


### Features

* add migrator without backoffice ([850720d](https://github.com/informatievlaanderen/address-registry/commit/850720dbc2e3491d4233a459ceb5d921397bf18f))

## [3.1.7](https://github.com/informatievlaanderen/address-registry/compare/v3.1.6...v3.1.7) (2022-04-08)


### Bug Fixes

* wms house-number-label ([30f2811](https://github.com/informatievlaanderen/address-registry/commit/30f28113c6dbc5a64e92f7a2b22f8c2945dc288e))

## [3.1.6](https://github.com/informatievlaanderen/address-registry/compare/v3.1.5...v3.1.6) (2022-04-06)


### Bug Fixes

* migration fix ([cc23b1c](https://github.com/informatievlaanderen/address-registry/commit/cc23b1c82e0406d9e20cb20a9438bef6c80806f3))

## [3.1.5](https://github.com/informatievlaanderen/address-registry/compare/v3.1.4...v3.1.5) (2022-04-06)


### Bug Fixes

* Computed column HouseNumberLabel ([3ed535e](https://github.com/informatievlaanderen/address-registry/commit/3ed535e96269a402c38ec7b69bd29d20e5b12639))

## [3.1.4](https://github.com/informatievlaanderen/address-registry/compare/v3.1.3...v3.1.4) (2022-04-06)


### Bug Fixes

* consumer ioc registration ([a1cf8c5](https://github.com/informatievlaanderen/address-registry/commit/a1cf8c5560d266f9d824d2697849a55e5081a992))

## [3.1.3](https://github.com/informatievlaanderen/address-registry/compare/v3.1.2...v3.1.3) (2022-04-06)


### Bug Fixes

* style trigger build ([480df42](https://github.com/informatievlaanderen/address-registry/commit/480df4237e45994fcb43f25768607b675326fe0d))

## [3.1.2](https://github.com/informatievlaanderen/address-registry/compare/v3.1.1...v3.1.2) (2022-04-06)


### Bug Fixes

* bugfix paused wms projection, add adres views, scaler & table-valued functions ([ac337c0](https://github.com/informatievlaanderen/address-registry/commit/ac337c0549d48aba23cd22226ac931dfdfe6458f))

## [3.1.1](https://github.com/informatievlaanderen/address-registry/compare/v3.1.0...v3.1.1) (2022-04-05)

# [3.1.0](https://github.com/informatievlaanderen/address-registry/compare/v3.0.1...v3.1.0) (2022-04-05)


### Features

* add consumer ([#556](https://github.com/informatievlaanderen/address-registry/issues/556)) ([cbd6fda](https://github.com/informatievlaanderen/address-registry/commit/cbd6fda4f2d8de139bef0879a451de9c099d5a1a))

## [3.0.1](https://github.com/informatievlaanderen/address-registry/compare/v3.0.0...v3.0.1) (2022-04-04)


### Bug Fixes

* set oslo context type to string GAWR-2931 ([dc8e8a1](https://github.com/informatievlaanderen/address-registry/commit/dc8e8a1bcbf584cf2ce36a5d8ab2c361197ef0c7))

# [3.0.0](https://github.com/informatievlaanderen/address-registry/compare/v2.38.0...v3.0.0) (2022-03-29)


### Bug Fixes

* dotnet-ef cli to 6.0.3 ([d06aa99](https://github.com/informatievlaanderen/address-registry/commit/d06aa998210f8121b496163d25ba0b1162591a74))
* specify parameter for dotnet version in main.yml ([3cd1231](https://github.com/informatievlaanderen/address-registry/commit/3cd123128c2be8f78ca62469054284c42a2857c1))
* style trigger build ([a2e4171](https://github.com/informatievlaanderen/address-registry/commit/a2e4171eb71e89e3fa971ea70e20f8b53ba12648))
* temp pin .net version ([212de63](https://github.com/informatievlaanderen/address-registry/commit/212de6355a9afeff25dd2c7dea29c0bdcb82cfd8))
* wms remove old migrations, add computed column ([0c4e37d](https://github.com/informatievlaanderen/address-registry/commit/0c4e37d269e9e529204e87b316f87fa21ac8f990))


### Features

* move to dotnet 6.0.3 ([83a0982](https://github.com/informatievlaanderen/address-registry/commit/83a098203e87e702a2f72bb1b57cc5a93a1f90ec))


### BREAKING CHANGES

* move to dotnet 6.0.3

# [2.38.0](https://github.com/informatievlaanderen/address-registry/compare/v2.37.0...v2.38.0) (2022-03-04)


### Features

* create wms projection views ([fbc4833](https://github.com/informatievlaanderen/address-registry/commit/fbc4833ac611fcf5a5af1e73fd610652e8672741))

# [2.37.0](https://github.com/informatievlaanderen/address-registry/compare/v2.36.0...v2.37.0) (2022-02-28)


### Features

* create wms projection ([4289b3a](https://github.com/informatievlaanderen/address-registry/commit/4289b3a976067e4a09752d9e10abc772d3dca24a))

# [2.36.0](https://github.com/informatievlaanderen/address-registry/compare/v2.35.3...v2.36.0) (2022-02-25)


### Features

* update api to 17.0.0 ([89658a6](https://github.com/informatievlaanderen/address-registry/commit/89658a643e1b2ecc962071e1779e089c3d9e6579))

## [2.35.3](https://github.com/informatievlaanderen/address-registry/compare/v2.35.2...v2.35.3) (2022-02-22)


### Bug Fixes

* rename AdresView column name VersionId to VersieId ([7fa21dc](https://github.com/informatievlaanderen/address-registry/commit/7fa21dc55e74a5ca5a1a8c66bc63ae5b89a62e5a))

## [2.35.2](https://github.com/informatievlaanderen/address-registry/compare/v2.35.1...v2.35.2) (2022-02-22)


### Bug Fixes

* GAWR-2754 & GAWR-2755 WFS AdresView ([6066a49](https://github.com/informatievlaanderen/address-registry/commit/6066a493e067d5a5162a390623cad6263c373fbb))

## [2.35.1](https://github.com/informatievlaanderen/address-registry/compare/v2.35.0...v2.35.1) (2022-02-16)


### Bug Fixes

* add schemabinding and remove view exists check ([2cbcf62](https://github.com/informatievlaanderen/address-registry/commit/2cbcf62d49fe43094e862cfe9ca36230769ffeef))
* create adresview sql migration (wfs projection) ([f7e6b00](https://github.com/informatievlaanderen/address-registry/commit/f7e6b0017fd6d2a3113d2b34f1d49535062b26e5))

# [2.35.0](https://github.com/informatievlaanderen/address-registry/compare/v2.34.8...v2.35.0) (2022-02-16)


### Features

* create wfs projection proj ([c3007ad](https://github.com/informatievlaanderen/address-registry/commit/c3007ad974f3c386b2b16d1f2fe62e6ae1c24da7))

## [2.34.8](https://github.com/informatievlaanderen/address-registry/compare/v2.34.7...v2.34.8) (2022-02-10)


### Bug Fixes

* update Api dependency to fix exception handler ([9b65eed](https://github.com/informatievlaanderen/address-registry/commit/9b65eedc0e4be9d4695e3923129957e42af56318))

## [2.34.7](https://github.com/informatievlaanderen/address-registry/compare/v2.34.6...v2.34.7) (2022-01-21)


### Bug Fixes

* correctly resume projections async ([8d476d1](https://github.com/informatievlaanderen/address-registry/commit/8d476d138b5e294d2b653b7ad440f923256543d6))

## [2.34.6](https://github.com/informatievlaanderen/address-registry/compare/v2.34.5...v2.34.6) (2022-01-18)

## [2.34.5](https://github.com/informatievlaanderen/address-registry/compare/v2.34.4...v2.34.5) (2022-01-17)


### Bug Fixes

* gawr-2576 2 kommas na komma bij 0 ([a46b5d6](https://github.com/informatievlaanderen/address-registry/commit/a46b5d6f76c7d3b87e05cd9d9dd65cdbafd71284))

## [2.34.4](https://github.com/informatievlaanderen/address-registry/compare/v2.34.3...v2.34.4) (2021-12-21)


### Bug Fixes

* api docs oslo endpoint ([9a75727](https://github.com/informatievlaanderen/address-registry/commit/9a7572702faa2fbe8d354e642618cbe6d6827690))

## [2.34.3](https://github.com/informatievlaanderen/address-registry/compare/v2.34.2...v2.34.3) (2021-12-21)


### Bug Fixes

* style trigger build ([dcb9092](https://github.com/informatievlaanderen/address-registry/commit/dcb9092a2297c944b000efb692caa2d4e396147d))

## [2.34.2](https://github.com/informatievlaanderen/address-registry/compare/v2.34.1...v2.34.2) (2021-12-17)


### Bug Fixes

* use async startup of projections to fix hanging migrations ([86380cc](https://github.com/informatievlaanderen/address-registry/commit/86380cc1e32d9b5d249c34f1a19d4160e621587e))

## [2.34.1](https://github.com/informatievlaanderen/address-registry/compare/v2.34.0...v2.34.1) (2021-12-14)


### Bug Fixes

* use Projector 10.2.5 ([cadb9bd](https://github.com/informatievlaanderen/address-registry/commit/cadb9bd5f6614f7635f59731948f688bdee46612))

# [2.34.0](https://github.com/informatievlaanderen/address-registry/compare/v2.33.0...v2.34.0) (2021-12-13)


### Features

* create oslo api for Address ([40fbac3](https://github.com/informatievlaanderen/address-registry/commit/40fbac3974a769e535355276b931c15992b1cf55))

# [2.33.0](https://github.com/informatievlaanderen/address-registry/compare/v2.32.20...v2.33.0) (2021-12-08)


### Features

* use new query/events endpoint ([cd7f696](https://github.com/informatievlaanderen/address-registry/commit/cd7f6967806946490d7560b94ec12f31fc908abd))

## [2.32.20](https://github.com/informatievlaanderen/address-registry/compare/v2.32.19...v2.32.20) (2021-11-02)


### Bug Fixes

* changed busnummer docu ([9b1ecdd](https://github.com/informatievlaanderen/address-registry/commit/9b1ecddde47a8826b84de828a46609a5386abe89))

## [2.32.19](https://github.com/informatievlaanderen/address-registry/compare/v2.32.18...v2.32.19) (2021-10-27)


### Bug Fixes

* trigger build ([d4c8653](https://github.com/informatievlaanderen/address-registry/commit/d4c865364328aa89f972d78db07893bf31fda9c6))

## [2.32.18](https://github.com/informatievlaanderen/address-registry/compare/v2.32.17...v2.32.18) (2021-10-25)


### Bug Fixes

* paket bump ([4a6c1dc](https://github.com/informatievlaanderen/address-registry/commit/4a6c1dcccf3e2c6519ca0822b6443cf3c844a268))
* removed dotsettings change ([744663a](https://github.com/informatievlaanderen/address-registry/commit/744663ae52dcf18ae280a097f791958ef6d3fcb9))

## [2.32.17](https://github.com/informatievlaanderen/address-registry/compare/v2.32.16...v2.32.17) (2021-10-21)


### Bug Fixes

* gawr-2202 api docu changes ([29e4e50](https://github.com/informatievlaanderen/address-registry/commit/29e4e50af7964fc843d94cdc8ec16334767ac7df))

## [2.32.16](https://github.com/informatievlaanderen/address-registry/compare/v2.32.15...v2.32.16) (2021-10-20)


### Bug Fixes

* bump projection-handling ([22969dc](https://github.com/informatievlaanderen/address-registry/commit/22969dc9a23de1cf809c5d1110d6276711809134))

## [2.32.15](https://github.com/informatievlaanderen/address-registry/compare/v2.32.14...v2.32.15) (2021-10-11)


### Bug Fixes

* removed dash from crab huisnummer and subadressid ([90d12af](https://github.com/informatievlaanderen/address-registry/commit/90d12af516045b59430252002d27ba527868c3fe))

## [2.32.14](https://github.com/informatievlaanderen/address-registry/compare/v2.32.13...v2.32.14) (2021-10-05)


### Bug Fixes

* build push test to ECR ([eaf6a77](https://github.com/informatievlaanderen/address-registry/commit/eaf6a778fd483910e29a818a7f123145476b1ad6))

## [2.32.13](https://github.com/informatievlaanderen/address-registry/compare/v2.32.12...v2.32.13) (2021-10-05)


### Bug Fixes

* push to ECR Test ([ae8bcae](https://github.com/informatievlaanderen/address-registry/commit/ae8bcaedafe4ecd20f37e535b0650d1516086409))

## [2.32.12](https://github.com/informatievlaanderen/address-registry/compare/v2.32.11...v2.32.12) (2021-10-05)


### Bug Fixes

* build push to ECR Test ([ca0ec03](https://github.com/informatievlaanderen/address-registry/commit/ca0ec03785f54c39949ff8a71a6cb273601cab3d))

## [2.32.11](https://github.com/informatievlaanderen/address-registry/compare/v2.32.10...v2.32.11) (2021-10-05)


### Bug Fixes

* gawr-615 versionid datetime offset+2 ([b966f98](https://github.com/informatievlaanderen/address-registry/commit/b966f98ce238014a62edc874f9761b6f2f4cadd9))

## [2.32.10](https://github.com/informatievlaanderen/address-registry/compare/v2.32.9...v2.32.10) (2021-10-01)


### Bug Fixes

* update package ([40a2371](https://github.com/informatievlaanderen/address-registry/commit/40a2371a34e0617f66e4ebe178c8ac3e0283c5ff))

## [2.32.9](https://github.com/informatievlaanderen/address-registry/compare/v2.32.8...v2.32.9) (2021-09-30)


### Bug Fixes

* add index for extract links and fix query gawr-2085 ([61c3b6e](https://github.com/informatievlaanderen/address-registry/commit/61c3b6e3e4af78790b7620bbccc727243582fff7))

## [2.32.8](https://github.com/informatievlaanderen/address-registry/compare/v2.32.7...v2.32.8) (2021-09-29)


### Bug Fixes

* gawr-621 api documentatie ([45743c9](https://github.com/informatievlaanderen/address-registry/commit/45743c9eb63978bc163effaa68e2c3798ac05181))

## [2.32.7](https://github.com/informatievlaanderen/address-registry/compare/v2.32.6...v2.32.7) (2021-09-28)


### Bug Fixes

* gawr-652 docfix address warning ([def2f48](https://github.com/informatievlaanderen/address-registry/commit/def2f48992ae617deff8d99a9d2b6c6cc64094a2))

## [2.32.6](https://github.com/informatievlaanderen/address-registry/compare/v2.32.5...v2.32.6) (2021-09-28)


### Bug Fixes

* extract buildingunitlinks no longer output unlinked buildingunits GAWR-2085 ([e3ff133](https://github.com/informatievlaanderen/address-registry/commit/e3ff133c81dfe1dda55f8b51cda64f5460dcf220))
* incomplete addresses no longer are included in linked extract GAWR-2088 ([7973b89](https://github.com/informatievlaanderen/address-registry/commit/7973b8957cdc4c09d33d9af8bbe79e01c23a5a69))

## [2.32.5](https://github.com/informatievlaanderen/address-registry/compare/v2.32.4...v2.32.5) (2021-09-27)


### Bug Fixes

* GAWR 604 doc fix crab huisnummer/subadres ([3c2138f](https://github.com/informatievlaanderen/address-registry/commit/3c2138f3666e4665797173c72d3c90984ad1d606))

## [2.32.4](https://github.com/informatievlaanderen/address-registry/compare/v2.32.3...v2.32.4) (2021-09-22)


### Bug Fixes

* gawr-611 fix exception detail ([edd157f](https://github.com/informatievlaanderen/address-registry/commit/edd157f9b272e1a3178310e850a99c97d1bce12d))

## [2.32.3](https://github.com/informatievlaanderen/address-registry/compare/v2.32.2...v2.32.3) (2021-09-20)


### Bug Fixes

* summary docs for postal ([f549be2](https://github.com/informatievlaanderen/address-registry/commit/f549be260020aa7be5a27f321c9d3adfd73070b9))

## [2.32.2](https://github.com/informatievlaanderen/address-registry/compare/v2.32.1...v2.32.2) (2021-09-16)


### Bug Fixes

* change jsonproperty to not required GAWR-613 ([065988e](https://github.com/informatievlaanderen/address-registry/commit/065988e9dacab54c7d80fbe8d2053bfd012ea8b9))

## [2.32.1](https://github.com/informatievlaanderen/address-registry/compare/v2.32.0...v2.32.1) (2021-08-26)


### Bug Fixes

* update grar-common dependencies GRAR-2060 ([ac632fd](https://github.com/informatievlaanderen/address-registry/commit/ac632fde16b3f30c1b6e350456d16c3ae0070cac))

# [2.32.0](https://github.com/informatievlaanderen/address-registry/compare/v2.31.15...v2.32.0) (2021-08-25)


### Features

* **extractcontroller:** add metadata file with latest event id to address extract ([b3dabea](https://github.com/informatievlaanderen/address-registry/commit/b3dabea8a406a059e306ce35efd9922e46133e6c))

## [2.31.15](https://github.com/informatievlaanderen/address-registry/compare/v2.31.14...v2.31.15) (2021-08-24)


### Bug Fixes

* extract links no longer gives empty linkedaddress id's GRAR-1912 ([7127bef](https://github.com/informatievlaanderen/address-registry/commit/7127bef9803ed3621d1601d4763a2bd1e7d8d1c9))

## [2.31.14](https://github.com/informatievlaanderen/address-registry/compare/v2.31.13...v2.31.14) (2021-08-11)


### Bug Fixes

* add logger to buildingunit sync ([67c7484](https://github.com/informatievlaanderen/address-registry/commit/67c748489cda0224161bfdcbe471a0d2a9d675f6))

## [2.31.13](https://github.com/informatievlaanderen/address-registry/compare/v2.31.12...v2.31.13) (2021-07-07)


### Bug Fixes

* correct remove building and add other buildingunit building actions GRAR-1912 ([0aacea3](https://github.com/informatievlaanderen/address-registry/commit/0aacea3c9c4a20cf72f254c5f10e952fbd884d88))

## [2.31.12](https://github.com/informatievlaanderen/address-registry/compare/v2.31.11...v2.31.12) (2021-06-25)


### Bug Fixes

* update aws DistributedMutex package ([b14e2db](https://github.com/informatievlaanderen/address-registry/commit/b14e2dbe8bd6ad74c4e4ab02fe4ec94a73be975f))

## [2.31.11](https://github.com/informatievlaanderen/address-registry/compare/v2.31.10...v2.31.11) (2021-06-25)


### Bug Fixes

* added unique constraint to the persistentlocalid ([0df1298](https://github.com/informatievlaanderen/address-registry/commit/0df12989c6509e9650c62cddcf11acbcc3d82b6d))

## [2.31.10](https://github.com/informatievlaanderen/address-registry/compare/v2.31.9...v2.31.10) (2021-06-17)


### Bug Fixes

* addressmatch validation now returns correct responsecode ([5a99304](https://github.com/informatievlaanderen/address-registry/commit/5a99304034b306080b39acb08e45d04ed9f88954))

## [2.31.9](https://github.com/informatievlaanderen/address-registry/compare/v2.31.8...v2.31.9) (2021-06-11)


### Bug Fixes

* get list of addresses without passing any filter ([ef22f92](https://github.com/informatievlaanderen/address-registry/commit/ef22f928301036aeff98613916dc2dc5e5e7a093))

## [2.31.8](https://github.com/informatievlaanderen/address-registry/compare/v2.31.7...v2.31.8) (2021-06-09)


### Bug Fixes

*  add niscode filter ([8dc4e77](https://github.com/informatievlaanderen/address-registry/commit/8dc4e776c98ad84609c0708722f25758bfe8442b))

## [2.31.7](https://github.com/informatievlaanderen/address-registry/compare/v2.31.6...v2.31.7) (2021-05-31)


### Bug Fixes

* update api ([f50a446](https://github.com/informatievlaanderen/address-registry/commit/f50a44697af205e8749155aaf2f352112da5b858))

## [2.31.6](https://github.com/informatievlaanderen/address-registry/compare/v2.31.5...v2.31.6) (2021-05-31)


### Bug Fixes

* update api and pipeline ([2570780](https://github.com/informatievlaanderen/address-registry/commit/2570780e782d6085de0c65e6b16f6e3a08878b1c))

## [2.31.5](https://github.com/informatievlaanderen/address-registry/compare/v2.31.4...v2.31.5) (2021-05-29)


### Bug Fixes

* move to 5.0.6 ([2e69382](https://github.com/informatievlaanderen/address-registry/commit/2e6938238c5e4baa6777c87e062b126d24dc3fcf))

## [2.31.4](https://github.com/informatievlaanderen/address-registry/compare/v2.31.3...v2.31.4) (2021-05-28)


### Bug Fixes

*  update string text ([36dcd81](https://github.com/informatievlaanderen/address-registry/commit/36dcd81860c0f9fbfce7bc00da61a7559d4c2220))

## [2.31.3](https://github.com/informatievlaanderen/address-registry/compare/v2.31.2...v2.31.3) (2021-05-26)


### Bug Fixes

* volledig adres optioneel maken GRAR-1904 ([207ef99](https://github.com/informatievlaanderen/address-registry/commit/207ef99543a1c1f6b1089658f0b9343d85793f2b))

## [2.31.2](https://github.com/informatievlaanderen/address-registry/compare/v2.31.1...v2.31.2) (2021-05-17)


### Bug Fixes

* correct syndication removed buildingunit ([4f9283b](https://github.com/informatievlaanderen/address-registry/commit/4f9283b06378fecdf6d355dea1024f1096faa08e))

## [2.31.1](https://github.com/informatievlaanderen/address-registry/compare/v2.31.0...v2.31.1) (2021-05-10)


### Bug Fixes

* correct provenance for persistentlocalid ([ee69223](https://github.com/informatievlaanderen/address-registry/commit/ee69223c2337a031a509ec9ffe4d67d0bf4e5c5f))

# [2.31.0](https://github.com/informatievlaanderen/address-registry/compare/v2.30.0...v2.31.0) (2021-05-04)


### Features

* bump packages ([39a8f47](https://github.com/informatievlaanderen/address-registry/commit/39a8f478903642afb6774ac74bf13d1c262d07ca))

# [2.30.0](https://github.com/informatievlaanderen/address-registry/compare/v2.29.1...v2.30.0) (2021-04-28)


### Features

* add status filter to legacy list ([2b96062](https://github.com/informatievlaanderen/address-registry/commit/2b960620a65e5b8e56711430ae08a030045867ab))

## [2.29.1](https://github.com/informatievlaanderen/address-registry/compare/v2.29.0...v2.29.1) (2021-04-26)


### Bug Fixes

* rename cache status endpoint in projector ([6e82d03](https://github.com/informatievlaanderen/address-registry/commit/6e82d036ad48f43ab447dde6c6b41b3ae3917d02))

# [2.29.0](https://github.com/informatievlaanderen/address-registry/compare/v2.28.1...v2.29.0) (2021-03-31)


### Bug Fixes

* only swallow specific exception ([d341cf4](https://github.com/informatievlaanderen/address-registry/commit/d341cf48f2b3d9d234ec2075afde42518a04c2c1))
* update docs projections ([02a4593](https://github.com/informatievlaanderen/address-registry/commit/02a459358c983080bd42d5f63d3daad477c92fa7))


### Features

* bump projector & projection handling ([dd77602](https://github.com/informatievlaanderen/address-registry/commit/dd77602c424d1ddf923dab533c3cad1e2ddab378))

## [2.28.1](https://github.com/informatievlaanderen/address-registry/compare/v2.28.0...v2.28.1) (2021-03-22)


### Bug Fixes

* remove ridingwolf, collaboration ended ([251496d](https://github.com/informatievlaanderen/address-registry/commit/251496dcbcae28779dfbeb230d30c71ae4917bd1))

# [2.28.0](https://github.com/informatievlaanderen/address-registry/compare/v2.27.3...v2.28.0) (2021-03-11)


### Bug Fixes

* update projector dependency GRAR-1876 ([801eb15](https://github.com/informatievlaanderen/address-registry/commit/801eb15de1a4882ae2b9de92cc154f49f39f676e))


### Features

* add projection attributes GRAR-1876 ([7da8c91](https://github.com/informatievlaanderen/address-registry/commit/7da8c91758feb4eaa96fa3fbad0cefa80664cd3b))

## [2.27.3](https://github.com/informatievlaanderen/address-registry/compare/v2.27.2...v2.27.3) (2021-03-10)


### Bug Fixes

* use isolation extract archive for extracts ([9d8d0b3](https://github.com/informatievlaanderen/address-registry/commit/9d8d0b32f78665e8c5b762dc7efaf87adfd18954))

## [2.27.2](https://github.com/informatievlaanderen/address-registry/compare/v2.27.1...v2.27.2) (2021-03-08)


### Bug Fixes

* remove addressversions GRAR-1876 ([5cdad4d](https://github.com/informatievlaanderen/address-registry/commit/5cdad4ded573317bcac8bae4ecd68d1863208b44))

## [2.27.1](https://github.com/informatievlaanderen/address-registry/compare/v2.27.0...v2.27.1) (2021-03-06)


### Bug Fixes

* disable retry strategy in extract ([23d87f5](https://github.com/informatievlaanderen/address-registry/commit/23d87f50469145e6c0bd53f78c17f2c0776351e5))

# [2.27.0](https://github.com/informatievlaanderen/address-registry/compare/v2.26.4...v2.27.0) (2021-03-05)


### Features

* add transaction isolation snapshot to extract GRAR-1796 ([543868a](https://github.com/informatievlaanderen/address-registry/commit/543868acf9f32bce6e04aaaf0c9bb062d4ff6f48))

## [2.26.4](https://github.com/informatievlaanderen/address-registry/compare/v2.26.3...v2.26.4) (2021-02-15)


### Bug Fixes

* register problem details helper for projector GRAR-1814 ([1bbbfe6](https://github.com/informatievlaanderen/address-registry/commit/1bbbfe63938ead1807724c938863b3213a4eb646))

## [2.26.3](https://github.com/informatievlaanderen/address-registry/compare/v2.26.2...v2.26.3) (2021-02-11)


### Bug Fixes

* update api with use of problemdetailshelper GRAR-1814 ([54cc7ce](https://github.com/informatievlaanderen/address-registry/commit/54cc7ced81b068ac3b1e45661560738d712e2b27))

## [2.26.2](https://github.com/informatievlaanderen/address-registry/compare/v2.26.1...v2.26.2) (2021-02-09)


### Bug Fixes

* correct puri for addressmatch when no streetname was found GRAR-1819 ([84500ef](https://github.com/informatievlaanderen/address-registry/commit/84500ef4ad221d4000d3c1fe6a94e711bce91253))

## [2.26.1](https://github.com/informatievlaanderen/address-registry/compare/v2.26.0...v2.26.1) (2021-02-02)


### Bug Fixes

* move to 5.0.2 ([7dc5b8e](https://github.com/informatievlaanderen/address-registry/commit/7dc5b8e425329cce3011aa74f1cf3c08e907ed91))

# [2.26.0](https://github.com/informatievlaanderen/address-registry/compare/v2.25.3...v2.26.0) (2021-01-30)


### Features

* add sync tag to events ([f6e1ca1](https://github.com/informatievlaanderen/address-registry/commit/f6e1ca15c2439f76b8baa8bcb7ae0538fe95eb03))

## [2.25.3](https://github.com/informatievlaanderen/address-registry/compare/v2.25.2...v2.25.3) (2021-01-29)


### Bug Fixes

* remove alternate sync links ([9759a31](https://github.com/informatievlaanderen/address-registry/commit/9759a3192611671702803679be7313ad352bff9d))

## [2.25.2](https://github.com/informatievlaanderen/address-registry/compare/v2.25.1...v2.25.2) (2021-01-19)


### Bug Fixes

* xml date serialization sync projection ([471e31e](https://github.com/informatievlaanderen/address-registry/commit/471e31e353572247104dd4ffbc8f7d6379f090ce))

## [2.25.1](https://github.com/informatievlaanderen/address-registry/compare/v2.25.0...v2.25.1) (2021-01-13)


### Bug Fixes

* import from crab relations before root are now handled correctly GRAR-1749 ([3c05c48](https://github.com/informatievlaanderen/address-registry/commit/3c05c489c9242f97b3a0d46588d78f343bdf1a2f))

# [2.25.0](https://github.com/informatievlaanderen/address-registry/compare/v2.24.5...v2.25.0) (2021-01-12)


### Features

* add syndication status to projector api GRAR-1567 ([20b445f](https://github.com/informatievlaanderen/address-registry/commit/20b445f20efdcba984f8292db73bc6fddea5848b))

## [2.24.5](https://github.com/informatievlaanderen/address-registry/compare/v2.24.4...v2.24.5) (2021-01-07)


### Bug Fixes

* add missing brace ([a0f5ca8](https://github.com/informatievlaanderen/address-registry/commit/a0f5ca807f829964fd5ec624ad491bc144b17f1e))
* improve cache status page GRAR-1734 ([ea44c52](https://github.com/informatievlaanderen/address-registry/commit/ea44c5251c9923a43fc13dcda5fafffa119299f2))

## [2.24.4](https://github.com/informatievlaanderen/address-registry/compare/v2.24.3...v2.24.4) (2021-01-07)


### Bug Fixes

* update deps ([455e47b](https://github.com/informatievlaanderen/address-registry/commit/455e47b9363851878faf3b6d72bb106d527c4573))

## [2.24.3](https://github.com/informatievlaanderen/address-registry/compare/v2.24.2...v2.24.3) (2020-12-28)


### Bug Fixes

* update basisregisters api dependency ([a8b840c](https://github.com/informatievlaanderen/address-registry/commit/a8b840c4bf4909df372c74897053f7ca4461db4d))

## [2.24.2](https://github.com/informatievlaanderen/address-registry/compare/v2.24.1...v2.24.2) (2020-12-21)


### Bug Fixes

* move to 5.0.1 ([52339b0](https://github.com/informatievlaanderen/address-registry/commit/52339b0a634079192e23a5f8e6a4153535305ab3))

## [2.24.1](https://github.com/informatievlaanderen/address-registry/compare/v2.24.0...v2.24.1) (2020-12-16)


### Bug Fixes

* unescape filter box- and housenumber for list address GRAR-1678 ([4886779](https://github.com/informatievlaanderen/address-registry/commit/4886779d232bc38cf6a2343e0824cde26795445c))

# [2.24.0](https://github.com/informatievlaanderen/address-registry/compare/v2.23.16...v2.24.0) (2020-12-15)


### Features

* add total count for crab endpoints ([bc648e7](https://github.com/informatievlaanderen/address-registry/commit/bc648e74941956eacf2e48de57994df8822279cb))

## [2.23.16](https://github.com/informatievlaanderen/address-registry/compare/v2.23.15...v2.23.16) (2020-12-14)


### Bug Fixes

* rename addressen to adressenIds in sync building ([09f4a92](https://github.com/informatievlaanderen/address-registry/commit/09f4a92a2aff091f050ba6a45d0c77e255787abe))

## [2.23.15](https://github.com/informatievlaanderen/address-registry/compare/v2.23.14...v2.23.15) (2020-12-10)


### Bug Fixes

* correct 7cffb606 to allow multiple niscodes ([aed36e3](https://github.com/informatievlaanderen/address-registry/commit/aed36e383828403cf06d5619cf349121b44bb608))

## [2.23.14](https://github.com/informatievlaanderen/address-registry/compare/v2.23.13...v2.23.14) (2020-12-10)


### Bug Fixes

* address list when no municipality is found break early ([1c8e171](https://github.com/informatievlaanderen/address-registry/commit/1c8e171d81685ecc59c21540af3fd9927ddaf536))

## [2.23.13](https://github.com/informatievlaanderen/address-registry/compare/v2.23.12...v2.23.13) (2020-12-09)


### Bug Fixes

* addressmatch fusion municipality with same street lookup GRAR-1642 ([20e2dc5](https://github.com/informatievlaanderen/address-registry/commit/20e2dc519ff7434a26dc03881818ca739c420dae))

## [2.23.12](https://github.com/informatievlaanderen/address-registry/compare/v2.23.11...v2.23.12) (2020-12-09)


### Bug Fixes

* build corrected of bump csv helper ([102690a](https://github.com/informatievlaanderen/address-registry/commit/102690a5f1b02464f4bc2ac29d08b83253eb6306))

## [2.23.11](https://github.com/informatievlaanderen/address-registry/compare/v2.23.10...v2.23.11) (2020-12-08)

## [2.23.10](https://github.com/informatievlaanderen/address-registry/compare/v2.23.9...v2.23.10) (2020-12-03)


### Bug Fixes

* remove IsComplete from parcel sync GRAR-1652 ([71e7326](https://github.com/informatievlaanderen/address-registry/commit/71e73264aa7ec9ac5865f4cb8b253afb8ef52e22))

## [2.23.9](https://github.com/informatievlaanderen/address-registry/compare/v2.23.8...v2.23.9) (2020-12-02)


### Bug Fixes

* rename Municipality syndication event GRAR-1650 ([ae7ba1b](https://github.com/informatievlaanderen/address-registry/commit/ae7ba1b14a18d59045c4616f357c39c31a5673df))
* rename Postal syndication event ([f511fcc](https://github.com/informatievlaanderen/address-registry/commit/f511fcc0007f0e2db12519c27083e801c14ff37d))

## [2.23.8](https://github.com/informatievlaanderen/address-registry/compare/v2.23.7...v2.23.8) (2020-11-23)


### Bug Fixes

* ignore deleted crab subaddress edit specific subaddress commands ([1a06a28](https://github.com/informatievlaanderen/address-registry/commit/1a06a28d845f8c9d308ff48824992b4e55ff4667))
* tests to reflect changes in commit 1a06a28d84 ([aa137c4](https://github.com/informatievlaanderen/address-registry/commit/aa137c49d6a10bb2995c51a6323837580cb06e70))

## [2.23.7](https://github.com/informatievlaanderen/address-registry/compare/v2.23.6...v2.23.7) (2020-11-18)


### Bug Fixes

* reattached parcel addresses didn't get synced ([ac5a714](https://github.com/informatievlaanderen/address-registry/commit/ac5a71430e838da2e521d028d11a27564b1d63b8))
* remove set-env usage in gh-actions ([9027fe7](https://github.com/informatievlaanderen/address-registry/commit/9027fe753bfa09b23af7664358032bc79c141026))

## [2.23.6](https://github.com/informatievlaanderen/address-registry/compare/v2.23.5...v2.23.6) (2020-11-16)


### Bug Fixes

* handle ParcelWasRecovered in syndication ([6d96182](https://github.com/informatievlaanderen/address-registry/commit/6d96182a1dc93e351e67b3a6be4675e282be69f8))

## [2.23.5](https://github.com/informatievlaanderen/address-registry/compare/v2.23.4...v2.23.5) (2020-11-13)


### Bug Fixes

* display sync response example as correct xml GRAR-1599 ([4761102](https://github.com/informatievlaanderen/address-registry/commit/4761102b0dae9efa66278fcee9bdb27e6084114c))
* set limit to default in example ([9879a20](https://github.com/informatievlaanderen/address-registry/commit/9879a207cad386d146ab75a4cf7dc28b16a4dd2c))
* upgrade swagger GRAR-1599 ([2f6de3e](https://github.com/informatievlaanderen/address-registry/commit/2f6de3e2bd68f45331f63e1da34c2c971cac149c))
* use production url for sync examples ([7ea8c56](https://github.com/informatievlaanderen/address-registry/commit/7ea8c56e01d059f0283c295f99610e5bcb606649))

## [2.23.4](https://github.com/informatievlaanderen/address-registry/compare/v2.23.3...v2.23.4) (2020-11-12)


### Bug Fixes

* set event name for register sync ([eb04ad5](https://github.com/informatievlaanderen/address-registry/commit/eb04ad5db41b8fa60a0b51af9b1e36bcd1fb3b69))
* use event name instead of event type name for xml sync element ([5a90be4](https://github.com/informatievlaanderen/address-registry/commit/5a90be46c36e33e0cd4680ea5e92239288995a79))

## [2.23.3](https://github.com/informatievlaanderen/address-registry/compare/v2.23.2...v2.23.3) (2020-11-09)


### Bug Fixes

* change public field to property GRAR-1636 ([7c799c4](https://github.com/informatievlaanderen/address-registry/commit/7c799c47471ed1f135bbea020dab5fe2b165922b))
* correct null status in sync object GRAR-1629 ([afd0a9c](https://github.com/informatievlaanderen/address-registry/commit/afd0a9cff86d3e6aea404fdc1a9ed7764dd0076b))

## [2.23.2](https://github.com/informatievlaanderen/address-registry/compare/v2.23.1...v2.23.2) (2020-11-09)


### Bug Fixes

* correct null objectid in sync object GRAR-1627 ([3afdd9d](https://github.com/informatievlaanderen/address-registry/commit/3afdd9d14ede623d20d40ff2c155c8a5034688eb))

## [2.23.1](https://github.com/informatievlaanderen/address-registry/compare/v2.23.0...v2.23.1) (2020-11-06)


### Bug Fixes

* logging ([f6564ea](https://github.com/informatievlaanderen/address-registry/commit/f6564ea1b05be39e6f8d39ab524ed6382f857dbb))
* logging ([0d65a72](https://github.com/informatievlaanderen/address-registry/commit/0d65a72376258f9c3a97cdba782b6c064e28d9bb))
* logging ([8cadbbc](https://github.com/informatievlaanderen/address-registry/commit/8cadbbca8a52acc675c43be5814fa6b843f39869))
* logging ([70c6056](https://github.com/informatievlaanderen/address-registry/commit/70c6056f3f0d71c1534ea315f464e4cdd7903faa))
* logging ([2a2af98](https://github.com/informatievlaanderen/address-registry/commit/2a2af98a48a0069cee20c78c9c2e8744bbe8fc80))
* logging ([7b47aa6](https://github.com/informatievlaanderen/address-registry/commit/7b47aa60785e47fa7746637ee0ce1d4dd64e0cfb))
* logging ([a910bda](https://github.com/informatievlaanderen/address-registry/commit/a910bda88f813a86787b5d399e7818868466f506))
* logging ([fd89541](https://github.com/informatievlaanderen/address-registry/commit/fd8954116e5636f4b1a37f153adcb376d9c072e4))
* serilog ([d6438c2](https://github.com/informatievlaanderen/address-registry/commit/d6438c263b8bfdb9b8a67c6197e54576e3c035aa))

# [2.23.0](https://github.com/informatievlaanderen/address-registry/compare/v2.22.0...v2.23.0) (2020-10-27)


### Features

* add error message for syndication projections ([e9710b9](https://github.com/informatievlaanderen/address-registry/commit/e9710b9ca8e9c46b41e912392bd24144edd15d64))

# [2.22.0](https://github.com/informatievlaanderen/address-registry/compare/v2.21.0...v2.22.0) (2020-10-26)


### Features

* update projector with gap detection and extended status api ([8925d9c](https://github.com/informatievlaanderen/address-registry/commit/8925d9c4a75897b63aa7de65c67dc10ad84af633))

# [2.21.0](https://github.com/informatievlaanderen/address-registry/compare/v2.20.0...v2.21.0) (2020-10-16)


### Features

* add cache status to projector api ([55759ad](https://github.com/informatievlaanderen/address-registry/commit/55759ade0fddc7d6bc3cefd0db2f1f77bdc0516c))

# [2.20.0](https://github.com/informatievlaanderen/address-registry/compare/v2.19.2...v2.20.0) (2020-10-14)


### Features

* add status to api legacy list ([3a4b1d4](https://github.com/informatievlaanderen/address-registry/commit/3a4b1d46ab463a8139ee0994c3bcb08f411a6c03))

## [2.19.2](https://github.com/informatievlaanderen/address-registry/compare/v2.19.1...v2.19.2) (2020-10-05)


### Bug Fixes

* run projection using the feedprojector GRAR-1562 ([c90d054](https://github.com/informatievlaanderen/address-registry/commit/c90d05495f093692c0d9a90623898f6b5832c929))

## [2.19.1](https://github.com/informatievlaanderen/address-registry/compare/v2.19.0...v2.19.1) (2020-09-28)


### Bug Fixes

* add null check to persistentlocalid addresslink syncs ([941eab7](https://github.com/informatievlaanderen/address-registry/commit/941eab7f86a4bd5c678527dcea102fbb3678c7a3))

# [2.19.0](https://github.com/informatievlaanderen/address-registry/compare/v2.18.6...v2.19.0) (2020-09-22)


### Features

* add import status endpoint GRAR-1400 ([37a2a75](https://github.com/informatievlaanderen/address-registry/commit/37a2a75a438d9985fd1ca139a362a32742602193))

## [2.18.6](https://github.com/informatievlaanderen/address-registry/compare/v2.18.5...v2.18.6) (2020-09-22)


### Bug Fixes

* AddressWasRemoved event is only applied once ([2bbee88](https://github.com/informatievlaanderen/address-registry/commit/2bbee88aa2b84f4537b1ac47af52311e2487e34e))
* move dockerfiles to 3.1.8 ([e07a8e0](https://github.com/informatievlaanderen/address-registry/commit/e07a8e085e1c563dda926cf71bfac46c30664996))
* move to 3.1.8 ([20dbe0c](https://github.com/informatievlaanderen/address-registry/commit/20dbe0cfc938566947f0363d9855e5b0ac8c9625))

## [2.18.5](https://github.com/informatievlaanderen/address-registry/compare/v2.18.4...v2.18.5) (2020-09-11)


### Bug Fixes

* remove Modification from xml GRAR-1529 ([1d58a54](https://github.com/informatievlaanderen/address-registry/commit/1d58a5404056ec09fe3f01e603c9b3e725e5aaa4))

## [2.18.4](https://github.com/informatievlaanderen/address-registry/compare/v2.18.3...v2.18.4) (2020-09-10)


### Bug Fixes

* add generator version GRAR-1540 ([4e0bc17](https://github.com/informatievlaanderen/address-registry/commit/4e0bc1798ff185dc17426046d0b2a82c15223b64))

## [2.18.3](https://github.com/informatievlaanderen/address-registry/compare/v2.18.2...v2.18.3) (2020-09-09)


### Bug Fixes

* add provenance when assigning PersistentLocalIdentifier GRAR-1532 ([33bca22](https://github.com/informatievlaanderen/address-registry/commit/33bca22796716919db30e416f4f76cc7e11263c7))
* correct provenance for CrabHouseNumberId GRAR-1532 ([d39729f](https://github.com/informatievlaanderen/address-registry/commit/d39729f1c40a0dfb0b2974987ad4e59aea39f55e))

## [2.18.2](https://github.com/informatievlaanderen/address-registry/compare/v2.18.1...v2.18.2) (2020-09-03)


### Bug Fixes

* sync null organisation defaults to unknown ([062002f](https://github.com/informatievlaanderen/address-registry/commit/062002f5aa81e8b48710579b4a8d53d817c7f698))

## [2.18.1](https://github.com/informatievlaanderen/address-registry/compare/v2.18.0...v2.18.1) (2020-09-02)


### Bug Fixes

* upgrade common to fix sync author ([dc790dd](https://github.com/informatievlaanderen/address-registry/commit/dc790ddae06f1bdf12dae3d0d447fabb0fc53ff0))

# [2.18.0](https://github.com/informatievlaanderen/address-registry/compare/v2.17.1...v2.18.0) (2020-09-02)


### Bug Fixes

* add crabvalidation exception handling ([6e56578](https://github.com/informatievlaanderen/address-registry/commit/6e56578c6e6eca739e288617ca08977e7cfcab87))
* add executionTime to lastobservedPosition ([be34106](https://github.com/informatievlaanderen/address-registry/commit/be34106225ce55b5ede7f10de4b40c7be18009aa))
* add lastobservedpostion to delete ([b4a00cb](https://github.com/informatievlaanderen/address-registry/commit/b4a00cb04ca7c3743c6929e3805753b5ee372aa5))
* add null checks ([4fa6ee4](https://github.com/informatievlaanderen/address-registry/commit/4fa6ee4ade3b9e0fad3e1e92cad51888e7d3ab7f))
* comment backoffice.api out for now ([0db86b8](https://github.com/informatievlaanderen/address-registry/commit/0db86b8b7cdf0a3b762c63132d7fdbca225a5acc))
* compare identifier on uri ([33ceaab](https://github.com/informatievlaanderen/address-registry/commit/33ceaaba5b0034e5f5a7a1920b55c4067dcade53))
* don't push backoffice container yet ([1111258](https://github.com/informatievlaanderen/address-registry/commit/1111258c54d66a118c00c6e870d055d0a7094df8))
* move geojson mapping to Crab ([d1c28ff](https://github.com/informatievlaanderen/address-registry/commit/d1c28ff97a9e046dc1519dadd7b898267eddaba9))
* move last observed position to API package ([218318f](https://github.com/informatievlaanderen/address-registry/commit/218318f148c80231f75f8160f632e202f0736bf5))
* update crabedit dependcies ([90739d0](https://github.com/informatievlaanderen/address-registry/commit/90739d07a0b83f467b191f6441f3dd08f4a587f3))
* update dependencies ([db36bcf](https://github.com/informatievlaanderen/address-registry/commit/db36bcf2c5ed7ecf82eac8c4c06b4dfc68fc5982))
* use geojson in request model ([f7f6d71](https://github.com/informatievlaanderen/address-registry/commit/f7f6d711e404e9f7de8e4fa9bd47a06bcf3472c2))
* use Oslo code list identifiers ([e5053cf](https://github.com/informatievlaanderen/address-registry/commit/e5053cf92e571d878647b85fab20bbe930a759e7))


### Features

* add basic change address ([59d5b07](https://github.com/informatievlaanderen/address-registry/commit/59d5b07805bced7bbb9a2bb5231e235c5cb48e71))
* add basic correct address ([a638926](https://github.com/informatievlaanderen/address-registry/commit/a63892612e566491badee1d7795edcdc6966be64))
* add delete address ([e765313](https://github.com/informatievlaanderen/address-registry/commit/e765313f93164d626fa2374ea9c574f7ec8deea1))
* add geojson to wkt mapping ([1c49d76](https://github.com/informatievlaanderen/address-registry/commit/1c49d761c7f4d2a4496d5616e61209ca500fd207))
* add last observed position to responses ([9afbb1b](https://github.com/informatievlaanderen/address-registry/commit/9afbb1bf2d0d2b8c0688fa0f0d8ddd1c530a2ac6))
* add subaddress ([62fdc3f](https://github.com/informatievlaanderen/address-registry/commit/62fdc3fdb07e73b1aec62377aae0b483e35c956b))
* add update/correct subaddress ([4aef66e](https://github.com/informatievlaanderen/address-registry/commit/4aef66e2a0de7277565299076dfb50b8f3a460c9))
* implement basic add housenumber request ([8f4617f](https://github.com/informatievlaanderen/address-registry/commit/8f4617fca180d842eb4ed36ecdfebc1cd98b87b0))
* return created address uri ([07fa827](https://github.com/informatievlaanderen/address-registry/commit/07fa827fbaa1da634f54dbf5b91e63d98e2a0cb5))
* setup backoffice api ([93d69bd](https://github.com/informatievlaanderen/address-registry/commit/93d69bd15db7986bdcdd89bd7272ea38daf14eb7))

## [2.17.1](https://github.com/informatievlaanderen/address-registry/compare/v2.17.0...v2.17.1) (2020-07-19)


### Bug Fixes

* move to 3.1.6 ([38b5680](https://github.com/informatievlaanderen/address-registry/commit/38b5680645f0f662a4744b2de11cb94934ff0850))

# [2.17.0](https://github.com/informatievlaanderen/address-registry/compare/v2.16.14...v2.17.0) (2020-07-14)


### Features

* add timestamp to sync provenance GRAR-1451 ([461520a](https://github.com/informatievlaanderen/address-registry/commit/461520aad3697430690be1f04539dbdc4435413a))

## [2.16.14](https://github.com/informatievlaanderen/address-registry/compare/v2.16.13...v2.16.14) (2020-07-13)


### Bug Fixes

* update dependencies ([91ac594](https://github.com/informatievlaanderen/address-registry/commit/91ac594d68ccc3594ec0027807dcf38c579fad9e))
* use typed embed value GRAR-1465 ([0727a1c](https://github.com/informatievlaanderen/address-registry/commit/0727a1cd1a637db98976dae6328f00f1aff62652))

## [2.16.13](https://github.com/informatievlaanderen/address-registry/compare/v2.16.12...v2.16.13) (2020-07-10)


### Bug Fixes

* correct author, links entry atom feed + example GRAR-1443 GRAR-1447 ([1839aca](https://github.com/informatievlaanderen/address-registry/commit/1839acaa5d294b9a9223759025905a84fd39cbd8))

## [2.16.12](https://github.com/informatievlaanderen/address-registry/compare/v2.16.11...v2.16.12) (2020-07-10)


### Bug Fixes

* enums were not correctly serialized in syndication event GRAR-1490 ([c211b04](https://github.com/informatievlaanderen/address-registry/commit/c211b04db3b836558a8a6cf048decab465d67d9d))

## [2.16.11](https://github.com/informatievlaanderen/address-registry/compare/v2.16.10...v2.16.11) (2020-07-09)


### Bug Fixes

* use natural sorting for equal scores GRAR-1477 ([e13b39a](https://github.com/informatievlaanderen/address-registry/commit/e13b39ab3c3e9b6e695aefe3814722554e83725f))

## [2.16.10](https://github.com/informatievlaanderen/address-registry/compare/v2.16.9...v2.16.10) (2020-07-07)


### Bug Fixes

* perform exact match before doing search match on street and muni GRAR-1482 ([bc71887](https://github.com/informatievlaanderen/address-registry/commit/bc71887ed64d6deb66a7f2e217cb5ae2c2ecd3c8))

## [2.16.9](https://github.com/informatievlaanderen/address-registry/compare/v2.16.8...v2.16.9) (2020-07-07)


### Bug Fixes

* cleanup user input on scoring GRAR-1478 ([3470a56](https://github.com/informatievlaanderen/address-registry/commit/3470a5673154bb833377e53fc2e08291b0da90ab))
* sort adresmatch on address after score GRAR-1477 ([0cbad2e](https://github.com/informatievlaanderen/address-registry/commit/0cbad2ee27c58fd0fd9f48cf1eae078c43c04c24))

## [2.16.8](https://github.com/informatievlaanderen/address-registry/compare/v2.16.7...v2.16.8) (2020-07-07)


### Bug Fixes

* testbestand adres representation GRAR-1481 ([1ef2492](https://github.com/informatievlaanderen/address-registry/commit/1ef24920bc6a039eff69ad6f3d420d9d1ce672af))

## [2.16.7](https://github.com/informatievlaanderen/address-registry/compare/v2.16.6...v2.16.7) (2020-07-03)


### Bug Fixes

* add SyndicationItemCreatedAt GRAR-1442 ([81b5525](https://github.com/informatievlaanderen/address-registry/commit/81b5525a4960f30f28dc42256a06851f293b387d))
* get updated value from projections GRAR-1442 ([c555e57](https://github.com/informatievlaanderen/address-registry/commit/c555e57586dd2bc1f486723a9be27038c30641c7))
* update dependencies ([2c4a6df](https://github.com/informatievlaanderen/address-registry/commit/2c4a6dffcd70e901d23cb54f34cae2cafcb1f10b))

## [2.16.6](https://github.com/informatievlaanderen/address-registry/compare/v2.16.5...v2.16.6) (2020-07-02)


### Bug Fixes

* improve adresmatch for merged municipalities ([b711ba7](https://github.com/informatievlaanderen/address-registry/commit/b711ba7abd2755085619454f173ffd57aafc471a))
* refactor tests to work with multilanguage GRAR-181 ([05e3397](https://github.com/informatievlaanderen/address-registry/commit/05e339704e7c7e43f65fd13661bd15c75fbda1b2))

## [2.16.5](https://github.com/informatievlaanderen/address-registry/compare/v2.16.4...v2.16.5) (2020-07-02)


### Bug Fixes

* improve scoring for bisnumbers GRAR-1463 ([6eb6d63](https://github.com/informatievlaanderen/address-registry/commit/6eb6d63cda6281d0f9f05c66491f57e7c4ec9d60))

## [2.16.4](https://github.com/informatievlaanderen/address-registry/compare/v2.16.3...v2.16.4) (2020-07-02)


### Bug Fixes

* extract address links add condition to join + stream parcel links ([78f5fdd](https://github.com/informatievlaanderen/address-registry/commit/78f5fdd914715fb95fbf84f4c3dddad75bef29b7))

## [2.16.3](https://github.com/informatievlaanderen/address-registry/compare/v2.16.2...v2.16.3) (2020-07-02)


### Bug Fixes

* always sort adresmatch results from best score to worst GRAR-1469 ([a95efd0](https://github.com/informatievlaanderen/address-registry/commit/a95efd0941737daa2fbe362a9c7dc4aabe39ba19))

## [2.16.2](https://github.com/informatievlaanderen/address-registry/compare/v2.16.1...v2.16.2) (2020-07-01)


### Bug Fixes

* add/remake indexes for extract addresslinks ([2066394](https://github.com/informatievlaanderen/address-registry/commit/206639488df8ce249c5d860a0fa1a6b10c299e1f))

## [2.16.1](https://github.com/informatievlaanderen/address-registry/compare/v2.16.0...v2.16.1) (2020-07-01)


### Bug Fixes

* add index on addresscomplete extract buildingunit ([84130f8](https://github.com/informatievlaanderen/address-registry/commit/84130f8ab21c1bd77f6639043c9a9047fb7f75d9))

# [2.16.0](https://github.com/informatievlaanderen/address-registry/compare/v2.15.10...v2.16.0) (2020-07-01)


### Features

* refactor metadata for atom feed GRAR-1436 GRAR-1445 GRAR-1453 GRAR-1455 ([f6c1579](https://github.com/informatievlaanderen/address-registry/commit/f6c1579eb9d31cc1c8c6439f2e13d70f1e9777b5))

## [2.15.10](https://github.com/informatievlaanderen/address-registry/compare/v2.15.9...v2.15.10) (2020-06-30)


### Bug Fixes

* clarify required fields validation error GRAR-385 ([432d29e](https://github.com/informatievlaanderen/address-registry/commit/432d29ea81a09c12457a1896fef5789a26f66134))
* dont throw warning for municipalities with identical names GRAR-406 ([a9dd168](https://github.com/informatievlaanderen/address-registry/commit/a9dd16840a1605af8c30e2ce3349b69760f5ca93))
* throw validation error if boxnumber is 40+ GRAR-364 ([8f8fa4d](https://github.com/informatievlaanderen/address-registry/commit/8f8fa4d7fefd21a5600cc20a66c947b8d5488a4f))
* throw validation error if niscode is 5+ GRAR-362 ([5473915](https://github.com/informatievlaanderen/address-registry/commit/54739150d6759fae3182a2d8be88a94d587a966b))
* throw validation error if postcode is 4+ GRAR-361 ([58b858c](https://github.com/informatievlaanderen/address-registry/commit/58b858c987bd6261323e36bca56a5cfc578a69d3))

## [2.15.9](https://github.com/informatievlaanderen/address-registry/compare/v2.15.8...v2.15.9) (2020-06-30)


### Bug Fixes

* add some dutch docs ([eea9704](https://github.com/informatievlaanderen/address-registry/commit/eea97047003062124dcc26b5250cc53e153c21c6))
* exclude trailing e on bisnummer suffix check ([ee2caf6](https://github.com/informatievlaanderen/address-registry/commit/ee2caf6dd9753673c9478fbea40f7ff512e84683))
* remove offset and add from to next uri GRAR-1418 ([4ff948f](https://github.com/informatievlaanderen/address-registry/commit/4ff948fcc7c07bed9f21dede03f40d019672b686))
* support housenumber ranges GRAR-1401 ([965b414](https://github.com/informatievlaanderen/address-registry/commit/965b414090f68842f6c471e2d4e08aa3f5753ebe))
* support lower scored matches when matching with boxnumber ([572e135](https://github.com/informatievlaanderen/address-registry/commit/572e135d31f06fd7c0a243e79cc8f6744e663a31))
* support matching 10 when you pass 10a GRAR-1461 ([fde94d0](https://github.com/informatievlaanderen/address-registry/commit/fde94d016fbc37e08cfa7e16d018c698e7b2af0e))
* support returning 10a when searching for 10 GRAR-1458 ([1d068bf](https://github.com/informatievlaanderen/address-registry/commit/1d068bf95e22bc87dc1f1136d3d8e0fe765d882e))
* support suffix e for housenumber GRAR-1461 ([ca583ef](https://github.com/informatievlaanderen/address-registry/commit/ca583efe25a8f1b0757a7abbf3a5a88d57cf4e67))

## [2.15.8](https://github.com/informatievlaanderen/address-registry/compare/v2.15.7...v2.15.8) (2020-06-30)


### Bug Fixes

* correct CRAB naming GRAR-1386 ([4144170](https://github.com/informatievlaanderen/address-registry/commit/41441701ef053ad4df534be09503f0e1fb0d2371))

## [2.15.7](https://github.com/informatievlaanderen/address-registry/compare/v2.15.6...v2.15.7) (2020-06-23)


### Bug Fixes

* configure baseurls for all problemdetails GRAR-1357 ([55ddefd](https://github.com/informatievlaanderen/address-registry/commit/55ddefdd54787daaa244923d30ef90cb8a4e2a09))
* configure baseurls for all problemdetails GRAR-1357 ([decd998](https://github.com/informatievlaanderen/address-registry/commit/decd9987f918be62bdc3ccca3830322771fa5e49))

## [2.15.6](https://github.com/informatievlaanderen/address-registry/compare/v2.15.5...v2.15.6) (2020-06-23)

## [2.15.5](https://github.com/informatievlaanderen/address-registry/compare/v2.15.4...v2.15.5) (2020-06-22)


### Bug Fixes

* configure baseurls for all problemdetails GRAR-1358 GRAR-1357 ([e22caca](https://github.com/informatievlaanderen/address-registry/commit/e22cacacb70fa4d01f20412e5fa91ded4290ea28))

## [2.15.4](https://github.com/informatievlaanderen/address-registry/compare/v2.15.3...v2.15.4) (2020-06-19)


### Bug Fixes

* move to 3.1.5 ([e7673d1](https://github.com/informatievlaanderen/address-registry/commit/e7673d1fb1282edc2a6d2d999d826591eaa2b9ec))
* move to 3.1.5 ([55627f5](https://github.com/informatievlaanderen/address-registry/commit/55627f5aca90bfd3bdb3d3a9e417138f4820b4ab))

## [2.15.3](https://github.com/informatievlaanderen/address-registry/compare/v2.15.2...v2.15.3) (2020-06-17)


### Bug Fixes

* optimized addresslist queries to reduce timeouts GRAR-1367 ([e7cc8b5](https://github.com/informatievlaanderen/address-registry/commit/e7cc8b5347d22f1971f750a944fd34452d0b82d1))

## [2.15.2](https://github.com/informatievlaanderen/address-registry/compare/v2.15.1...v2.15.2) (2020-06-12)


### Bug Fixes

* include streetnameid to list index ([e7e5211](https://github.com/informatievlaanderen/address-registry/commit/e7e5211f029c81b6a541e2f582eaa8c94440e724))

## [2.15.1](https://github.com/informatievlaanderen/address-registry/compare/v2.15.0...v2.15.1) (2020-06-10)


### Bug Fixes

* update grar extract GRAR-1330 ([d509b72](https://github.com/informatievlaanderen/address-registry/commit/d509b725a54708c29a5338e84fd68e087f32bb47))

# [2.15.0](https://github.com/informatievlaanderen/address-registry/compare/v2.14.3...v2.15.0) (2020-06-09)


### Features

* add retry on polling the syndication feed for the linked feeds ([09f7dd1](https://github.com/informatievlaanderen/address-registry/commit/09f7dd1ff3834d34fb6fe78b57b4982b01332f86))

## [2.14.3](https://github.com/informatievlaanderen/address-registry/compare/v2.14.2...v2.14.3) (2020-06-08)


### Bug Fixes

* build msil version for public api ([401566b](https://github.com/informatievlaanderen/address-registry/commit/401566b677890b69aa6fb4dda49c61bde1ba7588))

## [2.14.2](https://github.com/informatievlaanderen/address-registry/compare/v2.14.1...v2.14.2) (2020-05-30)


### Bug Fixes

* update dependencies GRAR-752 ([afc47e1](https://github.com/informatievlaanderen/address-registry/commit/afc47e1cdcdc63a8039f1ef70fd4d9bc6e136edb))

## [2.14.1](https://github.com/informatievlaanderen/address-registry/compare/v2.14.0...v2.14.1) (2020-05-27)


### Bug Fixes

* correct muni name search in addressmatch ([6da4384](https://github.com/informatievlaanderen/address-registry/commit/6da4384fc2da3f71bd238d98549f468256f3e175))

# [2.14.0](https://github.com/informatievlaanderen/address-registry/compare/v2.13.0...v2.14.0) (2020-05-26)


### Features

* add buildingunit extract linked address via syndication GRAR-853 ([e390e53](https://github.com/informatievlaanderen/address-registry/commit/e390e53340d36123d36062824db442f6102db127))

# [2.13.0](https://github.com/informatievlaanderen/address-registry/compare/v2.12.15...v2.13.0) (2020-05-22)


### Bug Fixes

* only run ci on master repo ([fac19ef](https://github.com/informatievlaanderen/address-registry/commit/fac19efaa7a1253f07c48cf43e7927b5c182500a))


### Features

* add prj file to address extract GRAR-356 ([695d4fc](https://github.com/informatievlaanderen/address-registry/commit/695d4fc05a7ca1c3a2a3f8d3bf32a2f02a679a34))

## [2.12.15](https://github.com/informatievlaanderen/address-registry/compare/v2.12.14...v2.12.15) (2020-05-20)


### Bug Fixes

* add build badge ([e33bff0](https://github.com/informatievlaanderen/address-registry/commit/e33bff0627dce22d3f255a73b1a473a9c47e2e10))

## [2.12.14](https://github.com/informatievlaanderen/address-registry/compare/v2.12.13...v2.12.14) (2020-05-20)


### Bug Fixes

* addressmatch postalcode without niscode now returns empty result ([bf53b36](https://github.com/informatievlaanderen/address-registry/commit/bf53b367e1ce05e977b60362b32780bc0849e7b8))
* upgrade csv helper ([56bfd7a](https://github.com/informatievlaanderen/address-registry/commit/56bfd7ac45f3c9f6eaee3e214b4747b5662c911d))

## [2.12.13](https://github.com/informatievlaanderen/address-registry/compare/v2.12.12...v2.12.13) (2020-05-19)


### Bug Fixes

* add cache warmer to docker push ([c62705f](https://github.com/informatievlaanderen/address-registry/commit/c62705f4d74837150adcbbaa349eeb7061a43eef))
* move to 3.1.4 and gh actions ([fd87a8c](https://github.com/informatievlaanderen/address-registry/commit/fd87a8c80dace63017abcca98ea55c46fe279d64))

## [2.12.12](https://github.com/informatievlaanderen/address-registry/compare/v2.12.11...v2.12.12) (2020-05-15)


### Bug Fixes

* correct default streetname according to muni language GR-1242 ([b7b4b02](https://github.com/informatievlaanderen/address-registry/commit/b7b4b02))

## [2.12.11](https://github.com/informatievlaanderen/address-registry/compare/v2.12.10...v2.12.11) (2020-05-06)


### Bug Fixes

* correct issues extract crabid's GRAR-1262 ([f219e7d](https://github.com/informatievlaanderen/address-registry/commit/f219e7d))

## [2.12.10](https://github.com/informatievlaanderen/address-registry/compare/v2.12.9...v2.12.10) (2020-05-06)

## [2.12.9](https://github.com/informatievlaanderen/address-registry/compare/v2.12.8...v2.12.9) (2020-05-05)

## [2.12.8](https://github.com/informatievlaanderen/address-registry/compare/v2.12.7...v2.12.8) (2020-05-05)


### Bug Fixes

* add extracts for crabid mappings ([d0ef37c](https://github.com/informatievlaanderen/address-registry/commit/d0ef37c))

## [2.12.7](https://github.com/informatievlaanderen/address-registry/compare/v2.12.6...v2.12.7) (2020-04-29)


### Bug Fixes

* add logo and licence info to nuget ([3d1eb97](https://github.com/informatievlaanderen/address-registry/commit/3d1eb97))

## [2.12.6](https://github.com/informatievlaanderen/address-registry/compare/v2.12.5...v2.12.6) (2020-04-28)


### Bug Fixes

* update grar dependencies GRAR-412 ([b949cfd](https://github.com/informatievlaanderen/address-registry/commit/b949cfd))

## [2.12.5](https://github.com/informatievlaanderen/address-registry/compare/v2.12.4...v2.12.5) (2020-04-25)


### Bug Fixes

* import subaddress updates housenumber applied correctly ([d11b9bc](https://github.com/informatievlaanderen/address-registry/commit/d11b9bc))

## [2.12.4](https://github.com/informatievlaanderen/address-registry/compare/v2.12.3...v2.12.4) (2020-04-14)


### Bug Fixes

* update import packages ([61137e5](https://github.com/informatievlaanderen/address-registry/commit/61137e5))

## [2.12.3](https://github.com/informatievlaanderen/address-registry/compare/v2.12.2...v2.12.3) (2020-04-10)


### Bug Fixes

* update packages ([40def15](https://github.com/informatievlaanderen/address-registry/commit/40def15))

## [2.12.2](https://github.com/informatievlaanderen/address-registry/compare/v2.12.1...v2.12.2) (2020-04-10)


### Bug Fixes

* update packages for import batch timestamps ([8ccb0d0](https://github.com/informatievlaanderen/address-registry/commit/8ccb0d0))
* update packages for import batch timestamps ([2106b67](https://github.com/informatievlaanderen/address-registry/commit/2106b67))

## [2.12.1](https://github.com/informatievlaanderen/address-registry/compare/v2.12.0...v2.12.1) (2020-04-06)


### Bug Fixes

* import new subaddress via update with older linked data ([baf7976](https://github.com/informatievlaanderen/address-registry/commit/baf7976))

# [2.12.0](https://github.com/informatievlaanderen/address-registry/compare/v2.11.4...v2.12.0) (2020-04-03)


### Features

* upgrade projection handling to include errmessage lastchangedlist ([3f95e48](https://github.com/informatievlaanderen/address-registry/commit/3f95e48))

## [2.11.4](https://github.com/informatievlaanderen/address-registry/compare/v2.11.3...v2.11.4) (2020-03-27)


### Bug Fixes

* don't output sync feed links if no persistent local id is present ([28ab11b](https://github.com/informatievlaanderen/address-registry/commit/28ab11b))

## [2.11.3](https://github.com/informatievlaanderen/address-registry/compare/v2.11.2...v2.11.3) (2020-03-27)


### Bug Fixes

* set sync feed dates to belgian timezone ([e50e483](https://github.com/informatievlaanderen/address-registry/commit/e50e483))

## [2.11.2](https://github.com/informatievlaanderen/address-registry/compare/v2.11.1...v2.11.2) (2020-03-25)


### Performance Improvements

* extract addresslinks only get ids from db to reduce memory ([dbbe366](https://github.com/informatievlaanderen/address-registry/commit/dbbe366))

## [2.11.1](https://github.com/informatievlaanderen/address-registry/compare/v2.11.0...v2.11.1) (2020-03-23)


### Bug Fixes

* correct versie id type in syndication ([eb2a2cb](https://github.com/informatievlaanderen/address-registry/commit/eb2a2cb))
* update grar common to fix versie id type ([640de86](https://github.com/informatievlaanderen/address-registry/commit/640de86))

# [2.11.0](https://github.com/informatievlaanderen/address-registry/compare/v2.10.2...v2.11.0) (2020-03-20)


### Features

* send mail when importer crashes ([f69e4eb](https://github.com/informatievlaanderen/address-registry/commit/f69e4eb))

## [2.10.2](https://github.com/informatievlaanderen/address-registry/compare/v2.10.1...v2.10.2) (2020-03-18)


### Bug Fixes

* use correct confluence user ([a6d2077](https://github.com/informatievlaanderen/address-registry/commit/a6d2077))

## [2.10.1](https://github.com/informatievlaanderen/address-registry/compare/v2.10.0...v2.10.1) (2020-03-17)


### Bug Fixes

* force build ([740912d](https://github.com/informatievlaanderen/address-registry/commit/740912d))

# [2.10.0](https://github.com/informatievlaanderen/address-registry/compare/v2.9.2...v2.10.0) (2020-03-17)


### Features

* upgrade importers to netcore3 ([623a5d4](https://github.com/informatievlaanderen/address-registry/commit/623a5d4))

## [2.9.2](https://github.com/informatievlaanderen/address-registry/compare/v2.9.1...v2.9.2) (2020-03-12)


### Bug Fixes

* bump api to fix validation problemdetails contract ([054f771](https://github.com/informatievlaanderen/address-registry/commit/054f771))

## [2.9.1](https://github.com/informatievlaanderen/address-registry/compare/v2.9.0...v2.9.1) (2020-03-11)


### Bug Fixes

* count addresses now counts correctly ([2f6cff0](https://github.com/informatievlaanderen/address-registry/commit/2f6cff0))

# [2.9.0](https://github.com/informatievlaanderen/address-registry/compare/v2.8.0...v2.9.0) (2020-03-10)


### Features

* add totaal aantal endpoint ([1a5db85](https://github.com/informatievlaanderen/address-registry/commit/1a5db85))

# [2.8.0](https://github.com/informatievlaanderen/address-registry/compare/v2.7.3...v2.8.0) (2020-03-10)


### Features

* use building directly for addressmatch (temp) ([a2127d5](https://github.com/informatievlaanderen/address-registry/commit/a2127d5))

## [2.7.3](https://github.com/informatievlaanderen/address-registry/compare/v2.7.2...v2.7.3) (2020-03-05)


### Bug Fixes

* set correct timestamp for sync projection ([0efc83d](https://github.com/informatievlaanderen/address-registry/commit/0efc83d))

## [2.7.2](https://github.com/informatievlaanderen/address-registry/compare/v2.7.1...v2.7.2) (2020-03-05)


### Bug Fixes

* update grar common to fix provenance ([aff898f](https://github.com/informatievlaanderen/address-registry/commit/aff898f))

## [2.7.1](https://github.com/informatievlaanderen/address-registry/compare/v2.7.0...v2.7.1) (2020-03-04)


### Bug Fixes

* bump netcore dockerfiles ([d23bd2d](https://github.com/informatievlaanderen/address-registry/commit/d23bd2d))

# [2.7.0](https://github.com/informatievlaanderen/address-registry/compare/v2.6.22...v2.7.0) (2020-03-03)


### Features

* upgrade netcoreapp31 and dependencies ([4c7ebfc](https://github.com/informatievlaanderen/address-registry/commit/4c7ebfc))

## [2.6.22](https://github.com/informatievlaanderen/address-registry/compare/v2.6.21...v2.6.22) (2020-03-03)


### Bug Fixes

* update dockerid detection ([44b0131](https://github.com/informatievlaanderen/address-registry/commit/44b0131))

## [2.6.21](https://github.com/informatievlaanderen/address-registry/compare/v2.6.20...v2.6.21) (2020-03-03)


### Bug Fixes

* bump netcore to 3.1.2 ([153dc49](https://github.com/informatievlaanderen/address-registry/commit/153dc49))
* update dockerid detection ([2bb63f4](https://github.com/informatievlaanderen/address-registry/commit/2bb63f4))

## [2.6.20](https://github.com/informatievlaanderen/address-registry/compare/v2.6.19...v2.6.20) (2020-02-27)


### Bug Fixes

* correct url in default configuration ([f09a7e0](https://github.com/informatievlaanderen/address-registry/commit/f09a7e0))
* update api dependencies for json serialization ([22ecac3](https://github.com/informatievlaanderen/address-registry/commit/22ecac3))

## [2.6.19](https://github.com/informatievlaanderen/address-registry/compare/v2.6.18...v2.6.19) (2020-02-26)


### Bug Fixes

* increase bosa result size to 1001 ([3e11800](https://github.com/informatievlaanderen/address-registry/commit/3e11800))

## [2.6.18](https://github.com/informatievlaanderen/address-registry/compare/v2.6.17...v2.6.18) (2020-02-24)


### Bug Fixes

* update projection handling & sync migrator ([49b1ae1](https://github.com/informatievlaanderen/address-registry/commit/49b1ae1))

## [2.6.17](https://github.com/informatievlaanderen/address-registry/compare/v2.6.16...v2.6.17) (2020-02-24)


### Bug Fixes

* increase migrator timeout ([dc157f3](https://github.com/informatievlaanderen/address-registry/commit/dc157f3))

## [2.6.16](https://github.com/informatievlaanderen/address-registry/compare/v2.6.15...v2.6.16) (2020-02-24)


### Bug Fixes

* no postal code now returns address ([27f9efd](https://github.com/informatievlaanderen/address-registry/commit/27f9efd))

## [2.6.15](https://github.com/informatievlaanderen/address-registry/compare/v2.6.14...v2.6.15) (2020-02-21)


### Performance Improvements

* increase performance by removing count from lists ([2b842b9](https://github.com/informatievlaanderen/address-registry/commit/2b842b9))

## [2.6.14](https://github.com/informatievlaanderen/address-registry/compare/v2.6.13...v2.6.14) (2020-02-20)


### Bug Fixes

* update grar common ([db32910](https://github.com/informatievlaanderen/address-registry/commit/db32910))

## [2.6.13](https://github.com/informatievlaanderen/address-registry/compare/v2.6.12...v2.6.13) (2020-02-19)


### Bug Fixes

* add order in api's + add clustered indexes ([8b87409](https://github.com/informatievlaanderen/address-registry/commit/8b87409))

## [2.6.12](https://github.com/informatievlaanderen/address-registry/compare/v2.6.11...v2.6.12) (2020-02-18)


### Bug Fixes

* nullability for addressmatch tables is now correct ([31cdd80](https://github.com/informatievlaanderen/address-registry/commit/31cdd80))

## [2.6.11](https://github.com/informatievlaanderen/address-registry/compare/v2.6.10...v2.6.11) (2020-02-17)


### Bug Fixes

* upgrade packages to fix json order ([3359927](https://github.com/informatievlaanderen/address-registry/commit/3359927))

## [2.6.10](https://github.com/informatievlaanderen/address-registry/compare/v2.6.9...v2.6.10) (2020-02-17)


### Bug Fixes

* upgrade Grar Common libs ([a3a202f](https://github.com/informatievlaanderen/address-registry/commit/a3a202f))

## [2.6.9](https://github.com/informatievlaanderen/address-registry/compare/v2.6.8...v2.6.9) (2020-02-14)


### Bug Fixes

* add index on list ([457bcf9](https://github.com/informatievlaanderen/address-registry/commit/457bcf9))

## [2.6.8](https://github.com/informatievlaanderen/address-registry/compare/v2.6.7...v2.6.8) (2020-02-14)


### Bug Fixes

* correct addressmatch response streetname url ([3a87e45](https://github.com/informatievlaanderen/address-registry/commit/3a87e45))
* filter addresses with no persistentlocalid from list ([d6bbf42](https://github.com/informatievlaanderen/address-registry/commit/d6bbf42))

## [2.6.7](https://github.com/informatievlaanderen/address-registry/compare/v2.6.6...v2.6.7) (2020-02-10)


### Bug Fixes

* JSON default value for nullable fields ([50ac4f4](https://github.com/informatievlaanderen/address-registry/commit/50ac4f4))

## [2.6.6](https://github.com/informatievlaanderen/address-registry/compare/v2.6.5...v2.6.6) (2020-02-06)


### Bug Fixes

* remove english text from addressmatch example ([4c1b04f](https://github.com/informatievlaanderen/address-registry/commit/4c1b04f))

## [2.6.5](https://github.com/informatievlaanderen/address-registry/compare/v2.6.4...v2.6.5) (2020-02-04)


### Bug Fixes

* force build ([bdb53f5](https://github.com/informatievlaanderen/address-registry/commit/bdb53f5))

## [2.6.4](https://github.com/informatievlaanderen/address-registry/compare/v2.6.3...v2.6.4) (2020-02-04)


### Bug Fixes

* update api for problemdetails fix ([facf7d6](https://github.com/informatievlaanderen/address-registry/commit/facf7d6))

## [2.6.3](https://github.com/informatievlaanderen/address-registry/compare/v2.6.2...v2.6.3) (2020-02-03)


### Bug Fixes

* add type to problemdetails ([a563b7e](https://github.com/informatievlaanderen/address-registry/commit/a563b7e))

## [2.6.2](https://github.com/informatievlaanderen/address-registry/compare/v2.6.1...v2.6.2) (2020-02-03)


### Bug Fixes

* addressmatch is one big bunch of nullables ([3854528](https://github.com/informatievlaanderen/address-registry/commit/3854528))

## [2.6.1](https://github.com/informatievlaanderen/address-registry/compare/v2.6.0...v2.6.1) (2020-02-03)


### Bug Fixes

* specify non nullable responses ([22d70af](https://github.com/informatievlaanderen/address-registry/commit/22d70af))

# [2.6.0](https://github.com/informatievlaanderen/address-registry/compare/v2.5.5...v2.6.0) (2020-02-01)


### Features

* upgrade netcoreapp31 and dependencies ([cdd8380](https://github.com/informatievlaanderen/address-registry/commit/cdd8380))

## [2.5.5](https://github.com/informatievlaanderen/address-registry/compare/v2.5.4...v2.5.5) (2020-01-31)


### Bug Fixes

* ef warning concerning local evaluation expression ([1190e0b](https://github.com/informatievlaanderen/address-registry/commit/1190e0b))

## [2.5.4](https://github.com/informatievlaanderen/address-registry/compare/v2.5.3...v2.5.4) (2020-01-30)


### Bug Fixes

* xml (gml) coordinates are now rounded to 11 digits ([2c4bcec](https://github.com/informatievlaanderen/address-registry/commit/2c4bcec))

## [2.5.3](https://github.com/informatievlaanderen/address-registry/compare/v2.5.2...v2.5.3) (2020-01-29)


### Bug Fixes

* update grar packages ([f00435d](https://github.com/informatievlaanderen/address-registry/commit/f00435d))

## [2.5.2](https://github.com/informatievlaanderen/address-registry/compare/v2.5.1...v2.5.2) (2020-01-28)


### Bug Fixes

* require dotnet cli ([459ceaa](https://github.com/informatievlaanderen/address-registry/commit/459ceaa))

## [2.5.1](https://github.com/informatievlaanderen/address-registry/compare/v2.5.0...v2.5.1) (2020-01-28)


### Bug Fixes

* dockerize cache warmer ([8a1da83](https://github.com/informatievlaanderen/address-registry/commit/8a1da83))
* force build ([b1ee523](https://github.com/informatievlaanderen/address-registry/commit/b1ee523))

# [2.5.0](https://github.com/informatievlaanderen/address-registry/compare/v2.4.5...v2.5.0) (2020-01-28)


### Features

* add CacheWarmer dockerfile ([c443012](https://github.com/informatievlaanderen/address-registry/commit/c443012))

## [2.4.5](https://github.com/informatievlaanderen/address-registry/compare/v2.4.4...v2.4.5) (2020-01-28)


### Bug Fixes

* BOSA filter when street and muni combined GR-1169 ([cbf5579](https://github.com/informatievlaanderen/address-registry/commit/cbf5579))

## [2.4.4](https://github.com/informatievlaanderen/address-registry/compare/v2.4.3...v2.4.4) (2020-01-28)


### Bug Fixes

* correct nullability on position address detail legacy ([8b22590](https://github.com/informatievlaanderen/address-registry/commit/8b22590))

## [2.4.3](https://github.com/informatievlaanderen/address-registry/compare/v2.4.2...v2.4.3) (2020-01-24)


### Bug Fixes

* ef core 3 unsupported client evaluation by making explicit ([1ea439a](https://github.com/informatievlaanderen/address-registry/commit/1ea439a))

## [2.4.2](https://github.com/informatievlaanderen/address-registry/compare/v2.4.1...v2.4.2) (2020-01-24)


### Bug Fixes

* views in addressmatch now map correctly ([0becdb6](https://github.com/informatievlaanderen/address-registry/commit/0becdb6))

## [2.4.1](https://github.com/informatievlaanderen/address-registry/compare/v2.4.0...v2.4.1) (2020-01-24)


### Bug Fixes

* add syndication to api references ([8e3fd78](https://github.com/informatievlaanderen/address-registry/commit/8e3fd78))

# [2.4.0](https://github.com/informatievlaanderen/address-registry/compare/v2.3.1...v2.4.0) (2020-01-23)


### Bug Fixes

* boxnumber was ignored in certain cases GR-1163 ([d10e59d](https://github.com/informatievlaanderen/address-registry/commit/d10e59d))


### Features

* add IsRemoved flag to syndications to process future events ([d9483a7](https://github.com/informatievlaanderen/address-registry/commit/d9483a7))

## [2.3.1](https://github.com/informatievlaanderen/address-registry/compare/v2.3.0...v2.3.1) (2020-01-23)


### Bug Fixes

* syndication distributed lock now runs async ([aa5053c](https://github.com/informatievlaanderen/address-registry/commit/aa5053c))

# [2.3.0](https://github.com/informatievlaanderen/address-registry/compare/v2.2.0...v2.3.0) (2020-01-23)


### Features

* upgrade projectionhandling package ([6c251bc](https://github.com/informatievlaanderen/address-registry/commit/6c251bc))

# [2.2.0](https://github.com/informatievlaanderen/address-registry/compare/v2.1.3...v2.2.0) (2020-01-23)


### Features

* use distributed lock ([ad2c0be](https://github.com/informatievlaanderen/address-registry/commit/ad2c0be))

## [2.1.3](https://github.com/informatievlaanderen/address-registry/compare/v2.1.2...v2.1.3) (2020-01-22)


### Bug Fixes

* add validation rule for rrstraatcode addressmatch GR-1162 ([bbd7066](https://github.com/informatievlaanderen/address-registry/commit/bbd7066))
* do not emit default values addressmatch GR-1156 ([1e5ecf3](https://github.com/informatievlaanderen/address-registry/commit/1e5ecf3))

## [2.1.2](https://github.com/informatievlaanderen/address-registry/compare/v2.1.1...v2.1.2) (2020-01-16)


### Bug Fixes

* get api's working again ([6869263](https://github.com/informatievlaanderen/address-registry/commit/6869263))

## [2.1.1](https://github.com/informatievlaanderen/address-registry/compare/v2.1.0...v2.1.1) (2020-01-03)


### Bug Fixes

* api output properties / names ([9686695](https://github.com/informatievlaanderen/address-registry/commit/9686695))

# [2.1.0](https://github.com/informatievlaanderen/address-registry/compare/v2.0.0...v2.1.0) (2020-01-03)


### Features

* allow only one projector instance ([0da579d](https://github.com/informatievlaanderen/address-registry/commit/0da579d))

# [2.0.0](https://github.com/informatievlaanderen/address-registry/compare/v1.18.5...v2.0.0) (2019-12-25)


### Bug Fixes

* update grar common for from/tobytes ([c976b3b](https://github.com/informatievlaanderen/address-registry/commit/c976b3b))


### Code Refactoring

* upgrade to netcoreapp31 ([30188ab](https://github.com/informatievlaanderen/address-registry/commit/30188ab))


### BREAKING CHANGES

* Upgrade to .NET Core 3.1

## [1.18.5](https://github.com/informatievlaanderen/address-registry/compare/v1.18.4...v1.18.5) (2019-12-19)


### Bug Fixes

* created one context for addressmatch + no multithread in cache ([3ad1bbe](https://github.com/informatievlaanderen/address-registry/commit/3ad1bbe))

## [1.18.4](https://github.com/informatievlaanderen/address-registry/compare/v1.18.3...v1.18.4) (2019-12-19)


### Performance Improvements

* trying to increase parcel sync performance ([91d689f](https://github.com/informatievlaanderen/address-registry/commit/91d689f))

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
