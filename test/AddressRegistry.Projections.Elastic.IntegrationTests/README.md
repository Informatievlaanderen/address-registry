# Test cases

## Only characters no spaces

### Input: `loppem`
Search for street names starting with and containing `loppem` in the address registry.

```bash
POST /tst_address_search_2/_search
{
  "size": 0,  // We set size to 0 because we don't want the full documents
  "query": {
    "nested": {
      "path": "streetName.names",
      "query": {
        "function_score": {
          "query": {
            "bool": {
              "should": [
                {
                  "constant_score": {
                    "filter": {
                      "prefix": {
                        "streetName.names.spelling": "loppem"
                      }
                    },
                    "boost": 5.0  // High boost for prefix match
                  }
                },
                {
                  "constant_score": {
                    "filter": {
                      "match": {
                        "streetName.names.spelling": "loppem"
                      }
                    },
                    "boost": 1.0  // Lower boost for match
                  }
                }
              ]
            }
          },
          "functions": [
            {
              "script_score": {
                "script": {
                  "source": "Math.max(0, _score - doc['streetName.names.spelling.keyword'].value.length() * 0.2)"
                }
              }
            }
          ],
          "boost_mode": "replace",  // Replace the original score with the script score
          "score_mode": "sum"  // Sum the function scores
        }
      }
    }
  },
  "aggs": {
    "unique_street_names": {
      "nested": {
        "path": "streetName.names"
      },
      "aggs": {
        "filtered_names": {
          "filter": {
            "bool": {
              "should": [
                {
                  "prefix": {
                    "streetName.names.spelling": "loppem"
                  }
                },
                {
                  "match": {
                    "streetName.names.spelling": "loppem"
                  }
                }
              ]
            }
          },
          "aggs": {
            "street_names": {
              "terms": {
                "field": "streetName.names.spelling.keyword",  // Use the keyword sub-field for aggregation
                "size": 10,
                "order": {
                  "top_hit_score": "desc"
                }
              },
              "aggs": {
                "top_street_name": {
                  "top_hits": {
                    "_source": {
                      "includes": ["streetName.names.spelling"]
                    },
                    "size": 1,
                    "sort": [
                      {
                        "_score": {
                          "order": "desc"
                        }
                      }
                    ]
                  }
                },
                "top_hit_score": {
                  "max": {
                    "script": "_score"
                  }
                }
              }
            }
          }
        }
      }
    }
  }
}
```
### Output
- Only return street names.
- `Loppemstraat` before `Woudweg naar Loppem` because of the higher boost for prefix match + reduce scoring of longer names.

## Only characters with spaces
### Input: `loppem zedel`
- Search for street names starting with and containing `loppem` AND search municipality names and postal names starting and containing `zedel` in the address registry.
- Search for street names starting with and containing `loppem zedel` in the address registry.

```bash
POST /tst_address_search_2/_search
{
  "size": 0,
  "query": {
    "bool": {
      "should": [
        {
          "nested": {
            "path": "streetName.names",
            "query": {
              "function_score": {
                "query": {
                  "bool": {
                    "should": [
                      {
                        "constant_score": {
                          "filter": {
                            "prefix": {
                              "streetName.names.spelling": "loppem"
                            }
                          },
                          "boost": 5.0
                        }
                      },
                      {
                        "constant_score": {
                          "filter": {
                            "match": {
                              "streetName.names.spelling": "loppem"
                            }
                          },
                          "boost": 1.0
                        }
                      }
                    ]
                  }
                },
                "functions": [
                  {
                    "script_score": {
                      "script": {
                        "source": "Math.max(0, _score - doc['streetName.names.spelling.keyword'].value.length() * 0.1)"
                      }
                    }
                  }
                ],
                "boost_mode": "replace",
                "score_mode": "sum"
              }
            }
          }
        },
        {
          "nested": {
            "path": "municipality.names",
            "query": {
              "bool": {
                "should": [
                  {
                    "constant_score": {
                      "filter": {
                        "prefix": {
                          "municipality.names.spelling": "zedel"
                        }
                      },
                      "boost": 5.0
                    }
                  },
                  {
                    "constant_score": {
                      "filter": {
                        "match": {
                          "municipality.names.spelling": "zedel"
                        }
                      },
                      "boost": 1.0
                    }
                  }
                ]
              }
            }
          }
        },
        {
          "nested": {
            "path": "postalInfo.names",
            "query": {
              "bool": {
                "should": [
                  {
                    "constant_score": {
                      "filter": {
                        "prefix": {
                          "postalInfo.names.spelling": "zedel"
                        }
                      },
                      "boost": 5.0
                    }
                  },
                  {
                    "constant_score": {
                      "filter": {
                        "match": {
                          "postalInfo.names.spelling": "zedel"
                        }
                      },
                      "boost": 1.0
                    }
                  }
                ]
              }
            }
          }
        },
        {
          "nested": {
            "path": "streetName.names",
            "query": {
              "function_score": {
                "query": {
                  "bool": {
                    "should": [
                      {
                        "constant_score": {
                          "filter": {
                            "prefix": {
                              "streetName.names.spelling": "loppem zedel"
                            }
                          },
                          "boost": 5.0
                        }
                      },
                      {
                        "constant_score": {
                          "filter": {
                            "match": {
                              "streetName.names.spelling": "loppem zedel"
                            }
                          },
                          "boost": 1.0
                        }
                      }
                    ]
                  }
                },
                "functions": [
                  {
                    "script_score": {
                      "script": {
                        "source": "Math.max(0, _score - doc['streetName.names.spelling.keyword'].value.length() * 0.1)"
                      }
                    }
                  }
                ],
                "boost_mode": "replace",
                "score_mode": "sum"
              }
            }
          }
        }
      ]
    }
  },
  "aggs": {
    "unique_street_names": {
      "nested": {
        "path": "streetName.names"
      },
      "aggs": {
        "filtered_names": {
          "filter": {
            "bool": {
              "should": [
                {
                  "prefix": {
                    "streetName.names.spelling": "loppem"
                  }
                },
                {
                  "match": {
                    "streetName.names.spelling": "loppem"
                  }
                },
                {
                  "prefix": {
                    "streetName.names.spelling": "loppem zedel"
                  }
                },
                {
                  "match": {
                    "streetName.names.spelling": "loppem zedel"
                  }
                }
              ]
            }
          },
          "aggs": {
            "street_names": {
              "terms": {
                "field": "streetName.names.spelling.keyword",
                "size": 10,
                "order": {
                  "top_hit_score": "desc"
                }
              },
              "aggs": {
                "top_street_name": {
                  "top_hits": {
                    "_source": {
                      "includes": ["streetName.names.spelling"]
                    },
                    "size": 1,
                    "sort": [
                      {
                        "_score": {
                          "order": "desc"
                        }
                      }
                    ]
                  }
                },
                "top_hit_score": {
                  "max": {
                    "script": "_score"
                  }
                }
              }
            }
          }
        }
      }
    }
  }
}
```

### Output
- Only return street names
- `Loppemstraat` before `Woudweg naar Loppem` because of the higher boost for prefix match + reduce scoring of longer names and matches with `zedel` in municipality and/or postal names.

## Only numbers
TBD (postcode ?)

## Characters _space_ numbers
### Input: `loppem 1`

Search for street names starting with and containing `loppem` AND search for house numbers starting with `1` in the address registry.

```bash
POST /tst_address_search_2/_search
{
  "size": 10,  // Number of top documents to return
  "query": {
    "bool": {
      "must": [  // Use must to enforce that both conditions are required
        {
          "bool": {
            "should": [
              {
                "constant_score": {
                  "filter": {
                    "term": {
                      "houseNumber": "1"  // Exact match for house number "1"
                    }
                  },
                  "boost": 2.0  // Higher boost for exact match
                }
              },
              {
                "constant_score": {
                  "filter": {
                    "prefix": {
                      "houseNumber": "1"  // Prefix match for house numbers starting with "1"
                    }
                  },
                  "boost": 1.0  // Lower boost for prefix match
                }
              }
            ]
          }
        },
        {
          "nested": {
            "path": "streetName.names",
            "query": {
              "function_score": {
                "query": {
                  "bool": {
                    "should": [
                      {
                        "constant_score": {
                          "filter": {
                            "prefix": {
                              "streetName.names.spelling": "loppem"
                            }
                          },
                          "boost": 5.0  // High boost for prefix match in street name
                        }
                      },
                      {
                        "constant_score": {
                          "filter": {
                            "match": {
                              "streetName.names.spelling": "loppem"
                            }
                          },
                          "boost": 1.0  // Lower boost for match in street name
                        }
                      }
                    ]
                  }
                },
                "functions": [
                  {
                    "script_score": {
                      "script": {
                        "source": "Math.max(0, _score - doc['streetName.names.spelling.keyword'].value.length() * 0.2)"
                      }
                    }
                  }
                ],
                "boost_mode": "replace",
                "score_mode": "sum"
              }
            }
          }
        }
      ]
    }
  },
  "sort": [
    {
      "_score": {
        "order": "desc"
      }
    }
  ]
}
```

### Output

- Return full addresses.
- `Loppemstraat 1` before `Loppemstraat 10` because of the higher boost for exact match on house number.

## Characters _space_ numbers _space_ characters

### Input: `loppem 1 zedel`

Search for street names starting with and containing `loppem` AND search for house numbers starting with `1` 
AND search for municipality names and postal names starting and containing `zedel` in the address registry.

```bash
POST /tst_address_search_2/_search
{
  "size": 10,  // Number of top documents to return
  "query": {
    "bool": {
      "must": [
        {
          "nested": {
            "path": "streetName.names",
            "query": {
              "function_score": {
                "query": {
                  "bool": {
                    "should": [
                      {
                        "constant_score": {
                          "filter": {
                            "prefix": {
                              "streetName.names.spelling": "loppem"
                            }
                          },
                          "boost": 5.0  // High boost for prefix match in street name
                        }
                      },
                      {
                        "constant_score": {
                          "filter": {
                            "match": {
                              "streetName.names.spelling": "loppem"
                            }
                          },
                          "boost": 1.0  // Lower boost for match in street name
                        }
                      }
                    ]
                  }
                },
                "functions": [
                  {
                    "script_score": {
                      "script": {
                        "source": "Math.max(0, _score - doc['streetName.names.spelling.keyword'].value.length() * 0.2)"
                      }
                    }
                  }
                ],
                "boost_mode": "replace",
                "score_mode": "sum"
              }
            }
          }
        },
        {
          "bool": {
            "should": [
              {
                "constant_score": {
                  "filter": {
                    "term": {
                      "houseNumber": "1"  // Exact match for house number "1"
                    }
                  },
                  "boost": 10.0  // Higher boost for exact match
                }
              },
              {
                "constant_score": {
                  "filter": {
                    "prefix": {
                      "houseNumber": "1"  // Prefix match for house numbers starting with "1"
                    }
                  },
                  "boost": 5.0  // Lower boost for prefix match
                }
              }
            ]
          }
        },
        {
          "bool": {
            "should": [
              {
                "nested": {
                  "path": "municipality.names",
                  "query": {
                    "bool": {
                      "should": [
                        {
                          "constant_score": {
                            "filter": {
                              "prefix": {
                                "municipality.names.spelling": "zedel"
                              }
                            },
                            "boost": 5.0  // High boost for prefix match in municipality name
                          }
                        },
                        {
                          "constant_score": {
                            "filter": {
                              "match": {
                                "municipality.names.spelling": "zedel"
                              }
                            },
                            "boost": 1.0  // Lower boost for match in municipality name
                          }
                        }
                      ]
                    }
                  }
                }
              },
              {
                "nested": {
                  "path": "postalInfo.names",
                  "query": {
                    "bool": {
                      "should": [
                        {
                          "constant_score": {
                            "filter": {
                              "prefix": {
                                "postalInfo.names.spelling": "zedel"
                              }
                            },
                            "boost": 5.0  // High boost for prefix match in postal name
                          }
                        },
                        {
                          "constant_score": {
                            "filter": {
                              "match": {
                                "postalInfo.names.spelling": "zedel"
                              }
                            },
                            "boost": 1.0  // Lower boost for match in postal name
                          }
                        }
                      ]
                    }
                  }
                }
              }
            ]
          }
        }
      ]
    }
  },
  "sort": [
    {
      "_score": {
        "order": "desc"
      }
    }
  ]
}
```

### Output
- Return full addresses.
- `Loppemstraat 1` before `Loppemstraat 10` because of the higher boost for exact match on house number and matches with `zedel` in municipality and/or postal names.

## Characters _space_ numbers _space_ bus_box_boite _space_ numbers

### Input: `loppem 1 bus 2`

Search for street names starting with and containing `loppem` AND search for house numbers starting with `1` AND search for box numbers starting with `2` in the address registry.

```bash
POST /tst_address_search_2/_search
{
  "size": 10,  // Number of top documents to return
  "query": {
    "bool": {
      "must": [  // Use must to enforce that all conditions are required
        {
          "bool": {
            "should": [
              {
                "constant_score": {
                  "filter": {
                    "term": {
                      "houseNumber": "1"  // Exact match for house number "1"
                    }
                  },
                  "boost": 5.0  // Higher boost for exact match
                }
              },
              {
                "constant_score": {
                  "filter": {
                    "prefix": {
                      "houseNumber": "1"  // Prefix match for house numbers starting with "1"
                    }
                  },
                  "boost": 1.0  // Lower boost for prefix match
                }
              }
            ]
          }
        },
        {
          "bool": {
            "should": [
              {
                "constant_score": {
                  "filter": {
                    "term": {
                      "boxNumber": "2"  // Exact match for box number "2"
                    }
                  },
                  "boost": 2.0  // Higher boost for exact match
                }
              },
              {
                "constant_score": {
                  "filter": {
                    "prefix": {
                      "boxNumber": "2"  // Prefix match for box numbers starting with "2"
                    }
                  },
                  "boost": 1.0  // Lower boost for prefix match
                }
              }
            ]
          }
        },
        {
          "nested": {
            "path": "streetName.names",
            "query": {
              "function_score": {
                "query": {
                  "bool": {
                    "should": [
                      {
                        "constant_score": {
                          "filter": {
                            "prefix": {
                              "streetName.names.spelling": "loppem"
                            }
                          },
                          "boost": 5.0  // High boost for prefix match in street name
                        }
                      },
                      {
                        "constant_score": {
                          "filter": {
                            "match": {
                              "streetName.names.spelling": "loppem"
                            }
                          },
                          "boost": 1.0  // Lower boost for match in street name
                        }
                      }
                    ]
                  }
                },
                "functions": [
                  {
                    "script_score": {
                      "script": {
                        "source": "Math.max(0, _score - doc['streetName.names.spelling.keyword'].value.length() * 0.2)"
                      }
                    }
                  }
                ],
                "boost_mode": "replace",
                "score_mode": "sum"
              }
            }
          }
        }
      ]
    }
  },
  "sort": [
    {
      "_score": {
        "order": "desc"
      }
    }
  ]
}
```

### Output
- Return full addresses.
- `Loppemstraat 1 bus 2` before `Loppemstraat 1 bus 10` because of the higher boost for exact match on house number and box number.

## Characters _space_ numbers _space_ numbers

### Input: `loppem 1 82`

Search for street names starting with and containing `loppem` AND search for house numbers starting with `1` AND search for postal codes starting with `82` in the address registry.

```bash
POST /tst_address_search_2/_search
{
  "size": 10,  // Number of top documents to return
  "query": {
    "bool": {
      "must": [
        {
          "nested": {
            "path": "streetName.names",
            "query": {
              "function_score": {
                "query": {
                  "bool": {
                    "should": [
                      {
                        "constant_score": {
                          "filter": {
                            "prefix": {
                              "streetName.names.spelling": "loppem"
                            }
                          },
                          "boost": 5.0  // High boost for prefix match in street name
                        }
                      },
                      {
                        "constant_score": {
                          "filter": {
                            "match": {
                              "streetName.names.spelling": "loppem"
                            }
                          },
                          "boost": 1.0  // Lower boost for match in street name
                        }
                      }
                    ]
                  }
                },
                "functions": [
                  {
                    "script_score": {
                      "script": {
                        "source": "Math.max(0, _score - doc['streetName.names.spelling.keyword'].value.length() * 0.2)"
                      }
                    }
                  }
                ],
                "boost_mode": "replace",
                "score_mode": "sum"
              }
            }
          }
        },
        {
          "bool": {
            "should": [
              {
                "constant_score": {
                  "filter": {
                    "term": {
                      "houseNumber": "1"  // Exact match for house number "1"
                    }
                  },
                  "boost": 10.0  // Higher boost for exact match
                }
              },
              {
                "constant_score": {
                  "filter": {
                    "prefix": {
                      "houseNumber": "1"  // Prefix match for house numbers starting with "1"
                    }
                  },
                  "boost": 5.0  // Lower boost for prefix match
                }
              }
            ]
          }
        },
        {
          "constant_score": {
            "filter": {
              "prefix": {
                "postalInfo.postalCode": "82"  // Prefix match for postal code starting with "82"
              }
            },
            "boost": 5.0  // Boost for postal code prefix match
          }
        }
      ]
    }
  },
  "sort": [
    {
      "_score": {
        "order": "desc"
      }
    }
  ]
}
```

### Output
- Return full addresses.
- `Loppemstraat 1` before `Loppemstraat 10` because of the higher boost for exact match on house number and matches with postal codes starting with `82`.

## Characters _space_ numbers _space_ bus_box_boite _space_ numbers _space_ numbers

### Input: `loppem 1 bus 1 82`

Search for street names starting with and containing `loppem` AND search for house numbers starting with `1` AND search for box numbers starting with `1` AND search for postal codes starting with `82` in the address registry.

```bash
POST /tst_address_search_2/_search
{
  "size": 10,  // Number of top documents to return
  "query": {
    "bool": {
      "must": [
        {
          "nested": {
            "path": "streetName.names",
            "query": {
              "function_score": {
                "query": {
                  "bool": {
                    "should": [
                      {
                        "constant_score": {
                          "filter": {
                            "prefix": {
                              "streetName.names.spelling": "loppem"
                            }
                          },
                          "boost": 5.0  // High boost for prefix match in street name
                        }
                      },
                      {
                        "constant_score": {
                          "filter": {
                            "match": {
                              "streetName.names.spelling": "loppem"
                            }
                          },
                          "boost": 1.0  // Lower boost for match in street name
                        }
                      }
                    ]
                  }
                },
                "functions": [
                  {
                    "script_score": {
                      "script": {
                        "source": "Math.max(0, _score - doc['streetName.names.spelling.keyword'].value.length() * 0.2)"
                      }
                    }
                  }
                ],
                "boost_mode": "replace",
                "score_mode": "sum"
              }
            }
          }
        },
        {
          "bool": {
            "should": [
              {
                "constant_score": {
                  "filter": {
                    "term": {
                      "houseNumber": "1"  // Exact match for house number "1"
                    }
                  },
                  "boost": 10.0  // Higher boost for exact match
                }
              },
              {
                "constant_score": {
                  "filter": {
                    "prefix": {
                      "houseNumber": "1"  // Prefix match for house numbers starting with "1"
                    }
                  },
                  "boost": 5.0  // Lower boost for prefix match
                }
              }
            ]
          }
        },
        {
          "bool": {
            "should": [
              {
                "constant_score": {
                  "filter": {
                    "term": {
                      "boxNumber": "1"  // Exact match for house number "1"
                    }
                  },
                  "boost": 10.0  // Higher boost for exact match
                }
              },
              {
                "constant_score": {
                  "filter": {
                    "prefix": {
                      "boxNumber": "1"  // Prefix match for house numbers starting with "1"
                    }
                  },
                  "boost": 5.0  // Lower boost for prefix match
                }
              }
            ]
          }
        },
        {
          "constant_score": {
            "filter": {
              "prefix": {
                "postalInfo.postalCode": "82"  // Prefix match for postal code starting with "82"
              }
            },
            "boost": 5.0  // Boost for postal code prefix match
          }
        }
      ]
    }
  },
  "sort": [
    {
      "_score": {
        "order": "desc"
      }
    }
  ]
}
```

### Output
- Return full addresses.
- `Loppemstraat 1 bus 1` before `Loppemstraat 1 bus 10` because of the higher boost for exact match on house number and box number and matches with postal codes starting with `82`.

## Characters _space_ numbers _space_ numbers _space_ characters

### Input: `loppem 1 8210 zedel`

Search for street names starting with and containing `loppem` AND search for house numbers starting with `1` AND search for postal codes starting with `8210` AND search for municipality names and postal names starting and containing `zedel` in the address registry.

```bash
POST /tst_address_search_2/_search
{
  "size": 10,  // Number of top documents to return
  "query": {
    "bool": {
      "must": [
        {
          "nested": {
            "path": "streetName.names",
            "query": {
              "function_score": {
                "query": {
                  "bool": {
                    "should": [
                      {
                        "constant_score": {
                          "filter": {
                            "prefix": {
                              "streetName.names.spelling": "loppem"
                            }
                          },
                          "boost": 5.0  // High boost for prefix match in street name
                        }
                      },
                      {
                        "constant_score": {
                          "filter": {
                            "match": {
                              "streetName.names.spelling": "loppem"
                            }
                          },
                          "boost": 1.0  // Lower boost for match in street name
                        }
                      }
                    ]
                  }
                },
                "functions": [
                  {
                    "script_score": {
                      "script": {
                        "source": "Math.max(0, _score - doc['streetName.names.spelling.keyword'].value.length() * 0.2)"
                      }
                    }
                  }
                ],
                "boost_mode": "replace",
                "score_mode": "sum"
              }
            }
          }
        },
        {
          "bool": {
            "should": [
              {
                "constant_score": {
                  "filter": {
                    "term": {
                      "houseNumber": "1"  // Exact match for house number "1"
                    }
                  },
                  "boost": 10.0  // Higher boost for exact match
                }
              },
              {
                "constant_score": {
                  "filter": {
                    "prefix": {
                      "houseNumber": "1"  // Prefix match for house numbers starting with "1"
                    }
                  },
                  "boost": 5.0  // Lower boost for prefix match
                }
              }
            ]
          }
        },
        {
          "constant_score": {
            "filter": {
              "prefix": {
                "postalInfo.postalCode": "8210"  // Prefix match
              }
            },
            "boost": 5.0  // Boost for postal code prefix match
          }
        },
        {
          "bool": {
            "should": [
              {
                "nested": {
                  "path": "municipality.names",
                  "query": {
                    "bool": {
                      "should": [
                        {
                          "constant_score": {
                            "filter": {
                              "prefix": {
                                "municipality.names.spelling": "zedel"
                              }
                            },
                            "boost": 5.0  // High boost for prefix match in municipality name
                          }
                        },
                        {
                          "constant_score": {
                            "filter": {
                              "match": {
                                "municipality.names.spelling": "zedel"
                              }
                            },
                            "boost": 1.0  // Lower boost for match in municipality name
                          }
                        }
                      ]
                    }
                  }
                }
              },
              {
                "nested": {
                  "path": "postalInfo.names",
                  "query": {
                    "bool": {
                      "should": [
                        {
                          "constant_score": {
                            "filter": {
                              "prefix": {
                                "postalInfo.names.spelling": "zedel"
                              }
                            },
                            "boost": 5.0  // High boost for prefix match in postal name
                          }
                        },
                        {
                          "constant_score": {
                            "filter": {
                              "match": {
                                "postalInfo.names.spelling": "zedel"
                              }
                            },
                            "boost": 1.0  // Lower boost for match in postal name
                          }
                        }
                      ]
                    }
                  }
                }
              }
            ]
          }
        }
      ]
    }
  },
  "sort": [
    {
      "_score": {
        "order": "desc"
      }
    }
  ]
}
```

### Output
- Return full addresses.
- `Loppemstraat 1` before `Loppemstraat 10` because of the higher boost for exact match on house number and box number and matches with postal codes starting with `8210` and `zedel` in municipality and/or postal names.

## Characters _space_ numbers _space_ bus_box_boite _space_ numbers _space_ numbers _space_ characters

### Input: `loppem 1 bus 1 8200 zedel`

Search for street names starting with and containing `loppem` AND search for house numbers starting with `1` AND search for box numbers starting with `2` AND search for postal codes starting with `8200` AND search for municipality names and postal names starting and containing `zedel` in the address registry.

```bash
POST /tst_address_search_2/_search
{
  "size": 10,  // Number of top documents to return
  "query": {
    "bool": {
      "must": [
        {
          "nested": {
            "path": "streetName.names",
            "query": {
              "function_score": {
                "query": {
                  "bool": {
                    "should": [
                      {
                        "constant_score": {
                          "filter": {
                            "prefix": {
                              "streetName.names.spelling": "loppem"
                            }
                          },
                          "boost": 5.0  // High boost for prefix match in street name
                        }
                      },
                      {
                        "constant_score": {
                          "filter": {
                            "match": {
                              "streetName.names.spelling": "loppem"
                            }
                          },
                          "boost": 1.0  // Lower boost for match in street name
                        }
                      }
                    ]
                  }
                },
                "functions": [
                  {
                    "script_score": {
                      "script": {
                        "source": "Math.max(0, _score - doc['streetName.names.spelling.keyword'].value.length() * 0.2)"
                      }
                    }
                  }
                ],
                "boost_mode": "replace",
                "score_mode": "sum"
              }
            }
          }
        },
        {
          "bool": {
            "should": [
              {
                "constant_score": {
                  "filter": {
                    "term": {
                      "houseNumber": "1"  // Exact match for house number "1"
                    }
                  },
                  "boost": 10.0  // Higher boost for exact match
                }
              },
              {
                "constant_score": {
                  "filter": {
                    "prefix": {
                      "houseNumber": "1"  // Prefix match for house numbers starting with "1"
                    }
                  },
                  "boost": 5.0  // Lower boost for prefix match
                }
              }
            ]
          }
        },
        {
          "bool": {
            "should": [
              {
                "constant_score": {
                  "filter": {
                    "term": {
                      "boxNumber": "1"  // Exact match for house number "1"
                    }
                  },
                  "boost": 10.0  // Higher boost for exact match
                }
              },
              {
                "constant_score": {
                  "filter": {
                    "prefix": {
                      "boxNumber": "1"  // Prefix match for house numbers starting with "1"
                    }
                  },
                  "boost": 5.0  // Lower boost for prefix match
                }
              }
            ]
          }
        },
        {
          "constant_score": {
            "filter": {
              "prefix": {
                "postalInfo.postalCode": "8210"  // Prefix match
              }
            },
            "boost": 5.0  // Boost for postal code prefix match
          }
        },
        {
          "bool": {
            "should": [
              {
                "nested": {
                  "path": "municipality.names",
                  "query": {
                    "bool": {
                      "should": [
                        {
                          "constant_score": {
                            "filter": {
                              "prefix": {
                                "municipality.names.spelling": "zedel"
                              }
                            },
                            "boost": 5.0  // High boost for prefix match in municipality name
                          }
                        },
                        {
                          "constant_score": {
                            "filter": {
                              "match": {
                                "municipality.names.spelling": "zedel"
                              }
                            },
                            "boost": 1.0  // Lower boost for match in municipality name
                          }
                        }
                      ]
                    }
                  }
                }
              },
              {
                "nested": {
                  "path": "postalInfo.names",
                  "query": {
                    "bool": {
                      "should": [
                        {
                          "constant_score": {
                            "filter": {
                              "prefix": {
                                "postalInfo.names.spelling": "zedel"
                              }
                            },
                            "boost": 5.0  // High boost for prefix match in postal name
                          }
                        },
                        {
                          "constant_score": {
                            "filter": {
                              "match": {
                                "postalInfo.names.spelling": "zedel"
                              }
                            },
                            "boost": 1.0  // Lower boost for match in postal name
                          }
                        }
                      ]
                    }
                  }
                }
              }
            ]
          }
        }
      ]
    }
  },
  "sort": [
    {
      "_score": {
        "order": "desc"
      }
    }
  ]
}
```

### Output
- Return full addresses.