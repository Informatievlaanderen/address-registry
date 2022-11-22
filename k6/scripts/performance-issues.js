import papaparse from 'https://jslib.k6.io/papaparse/5.1.1/index.js';
import { SharedArray } from "k6/data";
import http from 'k6/http';
import { check } from 'k6';
import { sleep } from "k6";

const csvData = new SharedArray("performance-issues-urls", function() 
{
  return papaparse.parse(open('performance-issues-urls.txt'), { header: false }).data;
});

export const options = 
{
    vus: 10,
    duration: '5m',
    // thresholds: 
    // {
    //       http_req_duration: ['p(95)<1000']
    // },
};

export default function() 
{
	let params = 
	{
        headers: 
		{ 
            'Accept': 'application/json'
        },
    };
	
	csvData.forEach(function(item)
    {
		if	(!item[0].startsWith("#") && item[0] != "")
		{
			let response = http.get(item[0], params);
			sleep(1);	
		}
    });
}