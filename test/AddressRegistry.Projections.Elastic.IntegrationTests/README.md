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

## Characters _space_ numbers _space_ bus_box_boite _space_ numbers

### Input: `loppem 1 bus 2`

## Characters _space_ numbers _space_ numbers

### Input: `loppem 1 8200`

## Characters _space_ numbers _space_ bus_box_boite _space_ numbers _space_ numbers

### Input: `loppem 1 bus 2 8200`

## Characters _space_ numbers _space_ numbers _space_ characters

### Input: `loppem 1 8200 zedel`

## Characters _space_ numbers _space_ bus_box_boite _space_ numbers _space_ numbers _space_ characters

### Input: `loppem 1 bus 2 8200 zedel`