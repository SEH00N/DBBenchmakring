<div style="width: 850px;margin: 0 auto;">

# <center>DB 벤치마킹 보고서</center>

<br/>

## 1. 개요

본 보고서는 Redis, MySQL, MongoDB 세 가지 데이터베이스에 대해 JSON, BSON, MemoryPack 직렬화 형식으로 데이터를 저장 및 조회했을 때의 성능을 비교 분석하기 위해 작성되었다. 실제 시스템 설계 시 데이터 형식 및 DB 선택에 대한 객관적 근거를 마련하는 것이 목적으로 한다.

모든 테스트는 하나의 WindowsOS PC 위에서 진행되며, 테스트 클라이언트는 .NET 8.0 C# 런타임을 통해 Windows (Windows 11 HOME) 환경에서, DB 서버는 WSL2 (Ubuntu 22.04 LTS) 환경에서 실행한다. 모든 테스트에 대하여 WSL2의 리소스는 동일한 기준으로 제한되며, 실제 클라이언트/서버 구조에 가까운 테스트 구성을 제공한다. 클라이언트와 서버 간 네트워크 연결은 로컬 루프백 주소(127.0.0.1)를 통해 이루어진다.

본 테스트는 각 모듈의 직렬화 및 역직렬화 과정에서 발생하는 오버헤드와 데이터베이스 자체의 처리 성능을 평가하기 위한 것으로, 네트워크 지연이나 커넥션 생성 비용 등의 외부 요인을 배제하기 위해 모든 데이터베이스 커넥션은 커넥션 풀 등을 통해 재활용한다.

<br/>

## 2. 벤치마크 환경

 - OS
   - Root OS : Windows 11 HOME
   - Client OS : Windows 11 HOME
   - DB Server OS : WSL2 (Ubuntu 22.04 LTS)
 - 하드웨어
   - CPU : AMD Ryzen 7 5800X 8-Core Processor
   - Memory : 64GB
   - Drive : SSD
 - WSL2 설정
   - Memory : 4GB
   - Processors : 2
   - Swap : 0
 - DB
   - Redis : Ver 8.0.0
   - MySQL : Ver 8.0.42
   - MongoDB : Ver 8.0.9
 - Client
   - C# RUNTIME : .NET 8.0 for Windows 64
   - JSON : System.Text.Json
   - BSON : MongoDB.Bson
   - MemoryPack : Cysharp.MemoryPack
   - Redis Client : StackExchange.Redis
   - MySQL Client : MySql.Data & Dapper
   - MongoDB Client : MongoDB.Driver

<br/>

## 3. 데이터 형식 및 구조

220,096 bytes (약 215 KB) 에 해당하는 데이터를 Serialize/Deserialize 및 Read/Write 하는 것으로 테스트 데이터를 설정한다.
```cs
[MemoryPackable]
public partial class TestData 
{
    // (72 bytes)
    public string UserID { get; set; } = "abc-0000-00000000000000-000000000000"; 

    // (16 bytes)
    public string UserName { get; set; } = "TestUser";
    
    // (8 bytes)
    public DateTime CreatedTime { get; set; } = DateTime.Now; 
    
    // 1000개 데이터 (4000 bytes)
    public float[] Floats { get; set; } = new float[] { 0.1f, 0.2f, 0.3f, ... }; 

    // 36글자 1000개 데이터 (72,000 bytes)
    public string[] Strings { get; set; } = new string[] { "abc-0000-00000000000000-000000000000", ... }; 

    // key: 36글자, value: 36글자 1000개 데이터 (144,000 bytes)
    public Dictionary<string, string> StringDictionary { get; set; } = new Dictionary<string, string>() {
        ["abc-0000-00000000000000-000000000000"] = "abc-0000-00000000000000-000000000000",
        ...
    };
}
```

<br/>

## 4. 테스트 항목 정의

테스트 구성은 다음과 같다.

|데이터베이스|데이터 포맷|직렬화/역직렬화 모듈|
|:-:|:-:|:-:|
|Redis|JSON|System.Text.Json|
|Redis|binary|Cysharp.MemoryPack|
|MySQL|JSON|System.Text.Json|
|MySQL|binary|Cysharp.MemoryPack|
|MongoDB|BSON|MongoDB.Bson|

<br/>

벤치마크 항목은 다음과 같다.
 1. 직렬화 / 역직렬화 오버헤드
 2. 직렬화 / 역직렬화 GC Alloc
 3. Read / Write 속도
 4. Read / Write GC Alloc
 5. 데이터 트래픽 (직렬화된 데이터 크기)

<br/>

테스트는 다음과 같은 순서로 진행한다.
 1. Write Test: 10000개의 데이터 Write (직렬화)
 2. Read Test: 10000개의 데이터 Read (역직렬화)
 3. Mixed Test: Read (역직렬화) 10000개, Write (직렬화) 10000개 비율의 혼합 처리

<br/>

## 5. 벤치마크 실행 방식

 - Stopwatch를 사용하여 밀리초 단위 측정한다.
 - 각 테스트 전 프로세스 재실행을 통해 메모리를 초기화한다.
 - 5회의 테스트를 반복하여 평균치를 결과로 도출한다.
 - 연결 풀 최소화하여 단일 요청 기준 측정한다.
 - 병렬 처리 없이 순차적 처리 기준으로 측정한다.
 - Lock과 Transaction등의 예외처리는 본 테스트에선 고려하지 않는다.
 - DB의 테이블은 미리 구성한 상태로 테스트를 진행한다.
 - 모든 데이터는 key=value 의 형식으로 저장한다.
 - 시스템 Warmup을 위해 1회의 모의 테스트를 진행한 후 본 테스트를 진행한다.
 - 버퍼 초기화를 위해 5회의 모의 Write/Read를 진행한 후 본 테스트를 진행한다.

<br/>

## 통계 집계 대상

|Serialize/Deserialize      |Read/Write                 |ETC                        |
|:-:                        |:-:                        |:-:                        |
|Processing Time Average    |Response Time Average      |Total Time                 |
|Processing Time Min        |Response Time Min          |Total GC Alloc             |
|Processing Time Max        |Response Time Max          |Serailized Data Size       |
|GC Alloc Average           |Response Time Median       |                           |
|GC Alloc Min               |Response Time 90th pct     |                           |
|GC Alloc Max               |Response Time 95th pct     |                           |
|                           |Response Time 99th pct     |                           |
|                           |GC Alloc Average           |                           |
|                           |GC Alloc Min               |                           |
|                           |GC Alloc Max               |                           |
|                           |Throughput req/s           |                           |

<br/>

## 벤치마크 결과

|JSON/Redis                           |MemoryPack/Redis                           |JSON/MySQL                           |MemoryPack/MySQL                           |BSON/MongoDB                           |
|:-:                                  |:-:                                        |:-:                                  |:-:                                        |:-:                                    |
|![](./Report/Colors/JSON_Redis.png)  |![](./Report/Colors/MemoryPack_Redis.png)  |![](./Report/Colors/JSON_MySQL.png)  |![](./Report/Colors/MemoryPack_MySQL.png)  |![](./Report/Colors/BSON_MongoDB.png)  |

### Processing Time (avg)
|Serailize(ms)                                                                  |Deserialize(ms)                                                                |
|:-:                                                                            |:-:                                                                            |
|<img src=".\Report\Charts\Performance_Serialize.png" style="width: 400px;"/>   |<img src=".\Report\Charts\Performance_Deserialize.png" style="width: 400px;"/> |

|Write(ms)                                                                      |Read(ms)                                                                       |
|:-:                                                                            |:-:                                                                            |
|<img src=".\Report\Charts\Performance_Write.png" style="width: 400px;"/>       |<img src=".\Report\Charts\Performance_Read.png" style="width: 400px;"/>        |

|Total(s)                                                                       |
|:-:                                                                            |
|<img src=".\Report\Charts\Performance_Total.png" style="width: 400px;"/>       |

---

### GC Alloc (avg)
|Serailize(mb)                                                                  |Deserialize(mb)                                                                |
|:-:                                                                            |:-:                                                                            |
|<img src=".\Report\Charts\GCAlloc_Serialize.png" style="width: 400px;"/>       |<img src=".\Report\Charts\GCAlloc_Deserialize.png" style="width: 400px;"/>     |

|Write(mb)                                                                      |Read(mb)                                                                       |
|:-:                                                                            |:-:                                                                            |
|<img src=".\Report\Charts\GCAlloc_Write.png" style="width: 400px;"/>           |<img src=".\Report\Charts\GCAlloc_Read.png" style="width: 400px;"/>            |

|Total(gb)                                                                      |
|:-:                                                                            |
|<img src=".\Report\Charts\GCAlloc_Total.png" style="width: 400px;"/>           |

---

### Throughput (req/s)
|Write                                                                          |Read                                                                           |
|:-:                                                                            |:-:                                                                            |
|<img src=".\Report\Charts\Throughput_Write.png" style="width: 400px;"/>        |<img src=".\Report\Charts\Throughput_Read.png" style="width: 400px;"/>         |

---

## Detail

[For Full Detail](https://github.com/SEH00N/DBBenchmarking/raw/refs/heads/main/Report/Report.xlsx)

### JSON/Redis
---

<table>
  <tr>
    <th colspan="6" style="text-align: center;">Serialize</th>
    <th colspan="6" style="text-align: center;">Deserialize</th>
  </tr>
  <tr>
    <th colspan="3" style="text-align: center;">ProcessingTime (ms)</th>
    <th colspan="3" style="text-align: center;">GC Alloc (mb)</th>
    <th colspan="3" style="text-align: center;">ProcessingTime (ms)</th>
    <th colspan="3" style="text-align: center;">GC Alloc (mb)</th>
  </tr>
  <tr>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
  </tr>
  <tr>
    <td style="text-align: center;">0.175</td>
    <td style="text-align: center;">0.151</td>
    <td style="text-align: center;">3.359</td>
    <td style="text-align: center;">0.231</td>
    <td style="text-align: center;">0.231</td>
    <td style="text-align: center;">0.731</td>
    <td style="text-align: center;">0.222</td>
    <td style="text-align: center;">0.196</td>
    <td style="text-align: center;">3.788</td>
    <td style="text-align: center;">0.518</td>
    <td style="text-align: center;">0.511</td>
    <td style="text-align: center;">1.019</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.178</td>
    <td style="text-align: center;">0.148</td>
    <td style="text-align: center;">3.794</td>
    <td style="text-align: center;">0.231</td>
    <td style="text-align: center;">0.231</td>
    <td style="text-align: center;">0.731</td>
    <td style="text-align: center;">0.216</td>
    <td style="text-align: center;">0.192</td>
    <td style="text-align: center;">2.055</td>
    <td style="text-align: center;">0.518</td>
    <td style="text-align: center;">0.511</td>
    <td style="text-align: center;">1.019</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.179</td>
    <td style="text-align: center;">0.151</td>
    <td style="text-align: center;">3.576</td>
    <td style="text-align: center;">0.231</td>
    <td style="text-align: center;">0.231</td>
    <td style="text-align: center;">0.731</td>
    <td style="text-align: center;">0.226</td>
    <td style="text-align: center;">0.2</td>
    <td style="text-align: center;">2.042</td>
    <td style="text-align: center;">0.518</td>
    <td style="text-align: center;">0.511</td>
    <td style="text-align: center;">1.022</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.179</td>
    <td style="text-align: center;">0.153</td>
    <td style="text-align: center;">3.591</td>
    <td style="text-align: center;">0.231</td>
    <td style="text-align: center;">0.231</td>
    <td style="text-align: center;">0.731</td>
    <td style="text-align: center;">0.221</td>
    <td style="text-align: center;">0.199</td>
    <td style="text-align: center;">2.008</td>
    <td style="text-align: center;">0.518</td>
    <td style="text-align: center;">0.511</td>
    <td style="text-align: center;">1.019</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.177</td>
    <td style="text-align: center;">0.15</td>
    <td style="text-align: center;">3.349</td>
    <td style="text-align: center;">0.231</td>
    <td style="text-align: center;">0.231</td>
    <td style="text-align: center;">0.731</td>
    <td style="text-align: center;">0.214</td>
    <td style="text-align: center;">0.195</td>
    <td style="text-align: center;">1.893</td>
    <td style="text-align: center;">0.518</td>
    <td style="text-align: center;">0.511</td>
    <td style="text-align: center;">1.019</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.178</td>
    <td style="text-align: center;">0.151</td>
    <td style="text-align: center;">3.534</td>
    <td style="text-align: center;">0.231</td>
    <td style="text-align: center;">0.231</td>
    <td style="text-align: center;">0.731</td>
    <td style="text-align: center;">0.22</td>
    <td style="text-align: center;">0.196</td>
    <td style="text-align: center;">2.357</td>
    <td style="text-align: center;">0.518</td>
    <td style="text-align: center;">0.511</td>
    <td style="text-align: center;">1.019</td>
  </tr>
</table>
<table>
  <tr>
    <th colspan="11" style="text-align: center;">Write</th>
  </tr>
  <tr>
    <th colspan="7" style="text-align: center;">Response Time (ms)</th>
    <th colspan="3" style="text-align: center;">GC Alloc (mb)</th>
    <th colspan="1" style="text-align: center;">Throughput</th>
  </tr>
  <tr>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">Median</th>
    <th style="text-align: center;">90th pct</th>
    <th style="text-align: center;">95th pct</th>
    <th style="text-align: center;">99th pct</th>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">req/s</th>
  </tr>
  <tr>
    <td style="text-align: center;">8.203</td>
    <td style="text-align: center;">0.241</td>
    <td style="text-align: center;">66.793</td>
    <td style="text-align: center;">0.336</td>
    <td style="text-align: center;">47.129</td>
    <td style="text-align: center;">48.959</td>
    <td style="text-align: center;">50.142</td>
    <td style="text-align: center;">0.001</td>
    <td style="text-align: center;">0</td>
    <td style="text-align: center;">0.078</td>
    <td style="text-align: center;">121.912</td>
  </tr>
  <tr>
    <td style="text-align: center;">9.047</td>
    <td style="text-align: center;">0.237</td>
    <td style="text-align: center;">76.945</td>
    <td style="text-align: center;">0.349</td>
    <td style="text-align: center;">47.634</td>
    <td style="text-align: center;">49.054</td>
    <td style="text-align: center;">50.101</td>
    <td style="text-align: center;">0.001</td>
    <td style="text-align: center;">0</td>
    <td style="text-align: center;">0.062</td>
    <td style="text-align: center;">110.536</td>
  </tr>
  <tr>
    <td style="text-align: center;">10.667</td>
    <td style="text-align: center;">0.258</td>
    <td style="text-align: center;">68.876</td>
    <td style="text-align: center;">0.354</td>
    <td style="text-align: center;">47.839</td>
    <td style="text-align: center;">49.101</td>
    <td style="text-align: center;">54.275</td>
    <td style="text-align: center;">0.001</td>
    <td style="text-align: center;">0</td>
    <td style="text-align: center;">0.078</td>
    <td style="text-align: center;">93.745</td>
  </tr>
  <tr>
    <td style="text-align: center;">9.363</td>
    <td style="text-align: center;">0.243</td>
    <td style="text-align: center;">68.08</td>
    <td style="text-align: center;">0.347</td>
    <td style="text-align: center;">47.639</td>
    <td style="text-align: center;">49.075</td>
    <td style="text-align: center;">50.431</td>
    <td style="text-align: center;">0.001</td>
    <td style="text-align: center;">0</td>
    <td style="text-align: center;">0.078</td>
    <td style="text-align: center;">106.805</td>
  </tr>
  <tr>
    <td style="text-align: center;">7.983</td>
    <td style="text-align: center;">0.24</td>
    <td style="text-align: center;">63.326</td>
    <td style="text-align: center;">0.333</td>
    <td style="text-align: center;">47.046</td>
    <td style="text-align: center;">48.997</td>
    <td style="text-align: center;">50.177</td>
    <td style="text-align: center;">0.001</td>
    <td style="text-align: center;">0</td>
    <td style="text-align: center;">0.031</td>
    <td style="text-align: center;">125.267</td>
  </tr>
  <tr>
    <td style="text-align: center;">9.052</td>
    <td style="text-align: center;">0.244</td>
    <td style="text-align: center;">68.804</td>
    <td style="text-align: center;">0.344</td>
    <td style="text-align: center;">47.457</td>
    <td style="text-align: center;">49.037</td>
    <td style="text-align: center;">51.025</td>
    <td style="text-align: center;">0.001</td>
    <td style="text-align: center;">0</td>
    <td style="text-align: center;">0.065</td>
    <td style="text-align: center;">111.653</td>
  </tr>
</table>
<table>
  <tr>
    <th colspan="11" style="text-align: center;">Read</th>
  </tr>
  <tr>
    <th colspan="7" style="text-align: center;">Response Time (ms)</th>
    <th colspan="3" style="text-align: center;">GC Alloc (mb)</th>
    <th colspan="1" style="text-align: center;">Throughput</th>
  </tr>
  <tr>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">Median</th>
    <th style="text-align: center;">90th pct</th>
    <th style="text-align: center;">95th pct</th>
    <th style="text-align: center;">99th pct</th>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">req/s</th>
  </tr>
  <tr>
    <td style="text-align: center;">0.35</td>
    <td style="text-align: center;">0.276</td>
    <td style="text-align: center;">2.393</td>
    <td style="text-align: center;">0.334</td>
    <td style="text-align: center;">0.401</td>
    <td style="text-align: center;">0.452</td>
    <td style="text-align: center;">0.543</td>
    <td style="text-align: center;">0.347</td>
    <td style="text-align: center;">0.346</td>
    <td style="text-align: center;">2.346</td>
    <td style="text-align: center;">2855.803</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.357</td>
    <td style="text-align: center;">0.28</td>
    <td style="text-align: center;">2.311</td>
    <td style="text-align: center;">0.34</td>
    <td style="text-align: center;">0.417</td>
    <td style="text-align: center;">0.468</td>
    <td style="text-align: center;">0.556</td>
    <td style="text-align: center;">0.347</td>
    <td style="text-align: center;">0.346</td>
    <td style="text-align: center;">2.346</td>
    <td style="text-align: center;">2799.875</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.391</td>
    <td style="text-align: center;">0.288</td>
    <td style="text-align: center;">1.714</td>
    <td style="text-align: center;">0.369</td>
    <td style="text-align: center;">0.478</td>
    <td style="text-align: center;">0.529</td>
    <td style="text-align: center;">0.686</td>
    <td style="text-align: center;">0.347</td>
    <td style="text-align: center;">0.346</td>
    <td style="text-align: center;">2.346</td>
    <td style="text-align: center;">2557.28</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.375</td>
    <td style="text-align: center;">0.289</td>
    <td style="text-align: center;">1.176</td>
    <td style="text-align: center;">0.359</td>
    <td style="text-align: center;">0.443</td>
    <td style="text-align: center;">0.494</td>
    <td style="text-align: center;">0.633</td>
    <td style="text-align: center;">0.347</td>
    <td style="text-align: center;">0.346</td>
    <td style="text-align: center;">2.346</td>
    <td style="text-align: center;">2664.37</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.352</td>
    <td style="text-align: center;">0.291</td>
    <td style="text-align: center;">1.353</td>
    <td style="text-align: center;">0.336</td>
    <td style="text-align: center;">0.4</td>
    <td style="text-align: center;">0.457</td>
    <td style="text-align: center;">0.578</td>
    <td style="text-align: center;">0.347</td>
    <td style="text-align: center;">0.346</td>
    <td style="text-align: center;">2.346</td>
    <td style="text-align: center;">2840.787</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.365</td>
    <td style="text-align: center;">0.285</td>
    <td style="text-align: center;">1.789</td>
    <td style="text-align: center;">0.348</td>
    <td style="text-align: center;">0.428</td>
    <td style="text-align: center;">0.48</td>
    <td style="text-align: center;">0.599</td>
    <td style="text-align: center;">0.347</td>
    <td style="text-align: center;">0.346</td>
    <td style="text-align: center;">2.346</td>
    <td style="text-align: center;">2743.623</td>
  </tr>
</table>
<table>
  <tr>
    <th colspan="3" style="text-align: center;">ETC</th>
  </tr>
  <tr>
    <th colspan="1" style="text-align: center;">Processing Time (ms)</th>
    <th colspan="1" style="text-align: center;">GC Alloc (mb)</th>
    <th colspan="1" style="text-align: center;">Serialized Data Size (kb)</th>
  </tr>
  <tr>
    <th style="text-align: center;">Total</th>
    <th style="text-align: center;">Total</th>
    <th style="text-align: center;">Single</th>
  </tr>
  <tr>
    <td style="text-align: center;">178994.849</td>
    <td style="text-align: center;">21949.969</td>
    <td style="text-align: center;">121058</td>
  </tr>
  <tr>
    <td style="text-align: center;">195965.566</td>
    <td style="text-align: center;">21949.875</td>
    <td style="text-align: center;">121057</td>
  </tr>
  <tr>
    <td style="text-align: center;">229251.315</td>
    <td style="text-align: center;">21949.197</td>
    <td style="text-align: center;">121058</td>
  </tr>
  <tr>
    <td style="text-align: center;">202775.671</td>
    <td style="text-align: center;">21948.548</td>
    <td style="text-align: center;">121058</td>
  </tr>
  <tr>
    <td style="text-align: center;">174511.316</td>
    <td style="text-align: center;">21949.456</td>
    <td style="text-align: center;">121058</td>
  </tr>
  <tr>
    <td style="text-align: center;">196299.743</td>
    <td style="text-align: center;">21949.409</td>
    <td style="text-align: center;">121057.8</td>
  </tr>
</table>

### MemoryPack/Redis
---

<table>
  <tr>
    <th colspan="6" style="text-align: center;">Serialize</th>
    <th colspan="6" style="text-align: center;">Deserialize</th>
  </tr>
  <tr>
    <th colspan="3" style="text-align: center;">ProcessingTime (ms)</th>
    <th colspan="3" style="text-align: center;">GC Alloc (mb)</th>
    <th colspan="3" style="text-align: center;">ProcessingTime (ms)</th>
    <th colspan="3" style="text-align: center;">GC Alloc (mb)</th>
  </tr>
  <tr>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
  </tr>
  <tr>
    <td style="text-align: center;">0.078</td>
    <td style="text-align: center;">0.056</td>
    <td style="text-align: center;">0.715</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">0.63</td>
    <td style="text-align: center;">0.131</td>
    <td style="text-align: center;">0.11</td>
    <td style="text-align: center;">1.247</td>
    <td style="text-align: center;">0.422</td>
    <td style="text-align: center;">0.418</td>
    <td style="text-align: center;">0.433</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.078</td>
    <td style="text-align: center;">0.057</td>
    <td style="text-align: center;">0.91</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">0.63</td>
    <td style="text-align: center;">0.123</td>
    <td style="text-align: center;">0.107</td>
    <td style="text-align: center;">0.629</td>
    <td style="text-align: center;">0.422</td>
    <td style="text-align: center;">0.418</td>
    <td style="text-align: center;">0.433</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.075</td>
    <td style="text-align: center;">0.057</td>
    <td style="text-align: center;">0.91</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">0.63</td>
    <td style="text-align: center;">0.118</td>
    <td style="text-align: center;">0.103</td>
    <td style="text-align: center;">0.775</td>
    <td style="text-align: center;">0.422</td>
    <td style="text-align: center;">0.418</td>
    <td style="text-align: center;">0.433</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.075</td>
    <td style="text-align: center;">0.056</td>
    <td style="text-align: center;">0.844</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">0.63</td>
    <td style="text-align: center;">0.119</td>
    <td style="text-align: center;">0.103</td>
    <td style="text-align: center;">0.601</td>
    <td style="text-align: center;">0.422</td>
    <td style="text-align: center;">0.418</td>
    <td style="text-align: center;">0.433</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.075</td>
    <td style="text-align: center;">0.056</td>
    <td style="text-align: center;">0.756</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">0.63</td>
    <td style="text-align: center;">0.121</td>
    <td style="text-align: center;">0.103</td>
    <td style="text-align: center;">0.605</td>
    <td style="text-align: center;">0.422</td>
    <td style="text-align: center;">0.418</td>
    <td style="text-align: center;">0.433</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.076</td>
    <td style="text-align: center;">0.056</td>
    <td style="text-align: center;">0.827</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">0.63</td>
    <td style="text-align: center;">0.122</td>
    <td style="text-align: center;">0.105</td>
    <td style="text-align: center;">0.771</td>
    <td style="text-align: center;">0.422</td>
    <td style="text-align: center;">0.418</td>
    <td style="text-align: center;">0.433</td>
  </tr>
</table>
<table>
  <tr>
    <th colspan="11" style="text-align: center;">Write</th>
  </tr>
  <tr>
    <th colspan="7" style="text-align: center;">Response Time (ms)</th>
    <th colspan="3" style="text-align: center;">GC Alloc (mb)</th>
    <th colspan="1" style="text-align: center;">Throughput</th>
  </tr>
  <tr>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">Median</th>
    <th style="text-align: center;">90th pct</th>
    <th style="text-align: center;">95th pct</th>
    <th style="text-align: center;">99th pct</th>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">req/s</th>
  </tr>
  <tr>
    <td style="text-align: center;">22.22</td>
    <td style="text-align: center;">0.253</td>
    <td style="text-align: center;">60.09</td>
    <td style="text-align: center;">0.544</td>
    <td style="text-align: center;">48.823</td>
    <td style="text-align: center;">49.413</td>
    <td style="text-align: center;">54.385</td>
    <td style="text-align: center;">0.001</td>
    <td style="text-align: center;">0</td>
    <td style="text-align: center;">0.078</td>
    <td style="text-align: center;">45.004</td>
  </tr>
  <tr>
    <td style="text-align: center;">22.353</td>
    <td style="text-align: center;">0.253</td>
    <td style="text-align: center;">60.376</td>
    <td style="text-align: center;">0.551</td>
    <td style="text-align: center;">49.02</td>
    <td style="text-align: center;">49.779</td>
    <td style="text-align: center;">54.685</td>
    <td style="text-align: center;">0</td>
    <td style="text-align: center;">0</td>
    <td style="text-align: center;">0.078</td>
    <td style="text-align: center;">44.737</td>
  </tr>
  <tr>
    <td style="text-align: center;">17.636</td>
    <td style="text-align: center;">0.236</td>
    <td style="text-align: center;">60.198</td>
    <td style="text-align: center;">0.415</td>
    <td style="text-align: center;">48.631</td>
    <td style="text-align: center;">49.597</td>
    <td style="text-align: center;">53.907</td>
    <td style="text-align: center;">0</td>
    <td style="text-align: center;">0</td>
    <td style="text-align: center;">0.078</td>
    <td style="text-align: center;">56.702</td>
  </tr>
  <tr>
    <td style="text-align: center;">20.461</td>
    <td style="text-align: center;">0.235</td>
    <td style="text-align: center;">60.539</td>
    <td style="text-align: center;">0.476</td>
    <td style="text-align: center;">49.102</td>
    <td style="text-align: center;">49.798</td>
    <td style="text-align: center;">54.486</td>
    <td style="text-align: center;">0</td>
    <td style="text-align: center;">0</td>
    <td style="text-align: center;">0.086</td>
    <td style="text-align: center;">48.873</td>
  </tr>
  <tr>
    <td style="text-align: center;">21.407</td>
    <td style="text-align: center;">0.245</td>
    <td style="text-align: center;">60.43</td>
    <td style="text-align: center;">0.493</td>
    <td style="text-align: center;">48.875</td>
    <td style="text-align: center;">49.802</td>
    <td style="text-align: center;">54.637</td>
    <td style="text-align: center;">0</td>
    <td style="text-align: center;">0</td>
    <td style="text-align: center;">0.078</td>
    <td style="text-align: center;">46.713</td>
  </tr>
  <tr>
    <td style="text-align: center;">20.816</td>
    <td style="text-align: center;">0.244</td>
    <td style="text-align: center;">60.327</td>
    <td style="text-align: center;">0.496</td>
    <td style="text-align: center;">48.89</td>
    <td style="text-align: center;">49.678</td>
    <td style="text-align: center;">54.42</td>
    <td style="text-align: center;">0</td>
    <td style="text-align: center;">0</td>
    <td style="text-align: center;">0.079</td>
    <td style="text-align: center;">48.406</td>
  </tr>
</table>
<table>
  <tr>
    <th colspan="11" style="text-align: center;">Read</th>
  </tr>
  <tr>
    <th colspan="7" style="text-align: center;">Response Time (ms)</th>
    <th colspan="3" style="text-align: center;">GC Alloc (mb)</th>
    <th colspan="1" style="text-align: center;">Throughput</th>
  </tr>
  <tr>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">Median</th>
    <th style="text-align: center;">90th pct</th>
    <th style="text-align: center;">95th pct</th>
    <th style="text-align: center;">99th pct</th>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">req/s</th>
  </tr>
  <tr>
    <td style="text-align: center;">0.411</td>
    <td style="text-align: center;">0.291</td>
    <td style="text-align: center;">3.129</td>
    <td style="text-align: center;">0.376</td>
    <td style="text-align: center;">0.535</td>
    <td style="text-align: center;">0.575</td>
    <td style="text-align: center;">0.706</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">2.13</td>
    <td style="text-align: center;">2430.187</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.38</td>
    <td style="text-align: center;">0.268</td>
    <td style="text-align: center;">2.676</td>
    <td style="text-align: center;">0.349</td>
    <td style="text-align: center;">0.499</td>
    <td style="text-align: center;">0.538</td>
    <td style="text-align: center;">0.672</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">2.13</td>
    <td style="text-align: center;">2633.918</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.384</td>
    <td style="text-align: center;">0.279</td>
    <td style="text-align: center;">302.533</td>
    <td style="text-align: center;">0.34</td>
    <td style="text-align: center;">0.488</td>
    <td style="text-align: center;">0.518</td>
    <td style="text-align: center;">0.635</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">2.13</td>
    <td style="text-align: center;">2604.351</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.371</td>
    <td style="text-align: center;">0.282</td>
    <td style="text-align: center;">2.586</td>
    <td style="text-align: center;">0.342</td>
    <td style="text-align: center;">0.49</td>
    <td style="text-align: center;">0.52</td>
    <td style="text-align: center;">0.646</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">2.13</td>
    <td style="text-align: center;">2698.08</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.382</td>
    <td style="text-align: center;">0.282</td>
    <td style="text-align: center;">3.863</td>
    <td style="text-align: center;">0.349</td>
    <td style="text-align: center;">0.5</td>
    <td style="text-align: center;">0.529</td>
    <td style="text-align: center;">0.66</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">2.13</td>
    <td style="text-align: center;">2619.622</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.385</td>
    <td style="text-align: center;">0.28</td>
    <td style="text-align: center;">62.957</td>
    <td style="text-align: center;">0.351</td>
    <td style="text-align: center;">0.502</td>
    <td style="text-align: center;">0.536</td>
    <td style="text-align: center;">0.664</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">2.13</td>
    <td style="text-align: center;">2597.232</td>
  </tr>
</table>
<table>
  <tr>
    <th colspan="3" style="text-align: center;">ETC</th>
  </tr>
  <tr>
    <th colspan="1" style="text-align: center;">Processing Time (ms)</th>
    <th colspan="1" style="text-align: center;">GC Alloc (mb)</th>
    <th colspan="1" style="text-align: center;">Serialized Data Size (kb)</th>
  </tr>
  <tr>
    <th style="text-align: center;">Total</th>
    <th style="text-align: center;">Total</th>
    <th style="text-align: center;">Single</th>
  </tr>
  <tr>
    <td style="text-align: center;">456814.785</td>
    <td style="text-align: center;">13659.943</td>
    <td style="text-align: center;">136081</td>
  </tr>
  <tr>
    <td style="text-align: center;">458661.662</td>
    <td style="text-align: center;">13659.539</td>
    <td style="text-align: center;">136081</td>
  </tr>
  <tr>
    <td style="text-align: center;">364261.971</td>
    <td style="text-align: center;">13661.887</td>
    <td style="text-align: center;">136081</td>
  </tr>
  <tr>
    <td style="text-align: center;">420520.222</td>
    <td style="text-align: center;">13660.038</td>
    <td style="text-align: center;">136081</td>
  </tr>
  <tr>
    <td style="text-align: center;">439682.109</td>
    <td style="text-align: center;">13658.242</td>
    <td style="text-align: center;">136081</td>
  </tr>
  <tr>
    <td style="text-align: center;">427988.15</td>
    <td style="text-align: center;">13659.93</td>
    <td style="text-align: center;">136081</td>
  </tr>
</table>

### JSON/MySQL
---

<table>
  <tr>
    <th colspan="6" style="text-align: center;">Serialize</th>
    <th colspan="6" style="text-align: center;">Deserialize</th>
  </tr>
  <tr>
    <th colspan="3" style="text-align: center;">ProcessingTime (ms)</th>
    <th colspan="3" style="text-align: center;">GC Alloc (mb)</th>
    <th colspan="3" style="text-align: center;">ProcessingTime (ms)</th>
    <th colspan="3" style="text-align: center;">GC Alloc (mb)</th>
  </tr>
  <tr>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
  </tr>
  <tr>
    <td style="text-align: center;">0.188</td>
    <td style="text-align: center;">0.151</td>
    <td style="text-align: center;">4.298</td>
    <td style="text-align: center;">0.231</td>
    <td style="text-align: center;">0.231</td>
    <td style="text-align: center;">0.731</td>
    <td style="text-align: center;">0.242</td>
    <td style="text-align: center;">0.207</td>
    <td style="text-align: center;">1.946</td>
    <td style="text-align: center;">0.518</td>
    <td style="text-align: center;">0.511</td>
    <td style="text-align: center;">1.019</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.184</td>
    <td style="text-align: center;">0.15</td>
    <td style="text-align: center;">4.02</td>
    <td style="text-align: center;">0.231</td>
    <td style="text-align: center;">0.231</td>
    <td style="text-align: center;">0.731</td>
    <td style="text-align: center;">0.241</td>
    <td style="text-align: center;">0.2</td>
    <td style="text-align: center;">2.423</td>
    <td style="text-align: center;">0.518</td>
    <td style="text-align: center;">0.511</td>
    <td style="text-align: center;">1.019</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.184</td>
    <td style="text-align: center;">0.149</td>
    <td style="text-align: center;">4.057</td>
    <td style="text-align: center;">0.231</td>
    <td style="text-align: center;">0.231</td>
    <td style="text-align: center;">0.731</td>
    <td style="text-align: center;">0.244</td>
    <td style="text-align: center;">0.206</td>
    <td style="text-align: center;">2.081</td>
    <td style="text-align: center;">0.518</td>
    <td style="text-align: center;">0.511</td>
    <td style="text-align: center;">1.019</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.185</td>
    <td style="text-align: center;">0.149</td>
    <td style="text-align: center;">3.924</td>
    <td style="text-align: center;">0.231</td>
    <td style="text-align: center;">0.231</td>
    <td style="text-align: center;">0.731</td>
    <td style="text-align: center;">0.241</td>
    <td style="text-align: center;">0.205</td>
    <td style="text-align: center;">2.248</td>
    <td style="text-align: center;">0.518</td>
    <td style="text-align: center;">0.511</td>
    <td style="text-align: center;">1.019</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.184</td>
    <td style="text-align: center;">0.149</td>
    <td style="text-align: center;">3.723</td>
    <td style="text-align: center;">0.231</td>
    <td style="text-align: center;">0.231</td>
    <td style="text-align: center;">0.731</td>
    <td style="text-align: center;">0.241</td>
    <td style="text-align: center;">0.206</td>
    <td style="text-align: center;">2.317</td>
    <td style="text-align: center;">0.518</td>
    <td style="text-align: center;">0.511</td>
    <td style="text-align: center;">1.019</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.185</td>
    <td style="text-align: center;">0.15</td>
    <td style="text-align: center;">4.005</td>
    <td style="text-align: center;">0.231</td>
    <td style="text-align: center;">0.231</td>
    <td style="text-align: center;">0.731</td>
    <td style="text-align: center;">0.242</td>
    <td style="text-align: center;">0.205</td>
    <td style="text-align: center;">2.203</td>
    <td style="text-align: center;">0.518</td>
    <td style="text-align: center;">0.511</td>
    <td style="text-align: center;">1.019</td>
  </tr>
</table>
<table>
  <tr>
    <th colspan="11" style="text-align: center;">Write</th>
  </tr>
  <tr>
    <th colspan="7" style="text-align: center;">Response Time (ms)</th>
    <th colspan="3" style="text-align: center;">GC Alloc (mb)</th>
    <th colspan="1" style="text-align: center;">Throughput</th>
  </tr>
  <tr>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">Median</th>
    <th style="text-align: center;">90th pct</th>
    <th style="text-align: center;">95th pct</th>
    <th style="text-align: center;">99th pct</th>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">req/s</th>
  </tr>
  <tr>
    <td style="text-align: center;">52.303</td>
    <td style="text-align: center;">4.074</td>
    <td style="text-align: center;">308.085</td>
    <td style="text-align: center;">49.135</td>
    <td style="text-align: center;">69.009</td>
    <td style="text-align: center;">73.784</td>
    <td style="text-align: center;">86.762</td>
    <td style="text-align: center;">2.096</td>
    <td style="text-align: center;">2.092</td>
    <td style="text-align: center;">2.106</td>
    <td style="text-align: center;">19.119</td>
  </tr>
  <tr>
    <td style="text-align: center;">51.984</td>
    <td style="text-align: center;">4.009</td>
    <td style="text-align: center;">484.561</td>
    <td style="text-align: center;">48.687</td>
    <td style="text-align: center;">68.492</td>
    <td style="text-align: center;">73.257</td>
    <td style="text-align: center;">86.652</td>
    <td style="text-align: center;">2.096</td>
    <td style="text-align: center;">2.092</td>
    <td style="text-align: center;">2.106</td>
    <td style="text-align: center;">19.237</td>
  </tr>
  <tr>
    <td style="text-align: center;">51.704</td>
    <td style="text-align: center;">4.195</td>
    <td style="text-align: center;">419.844</td>
    <td style="text-align: center;">48.577</td>
    <td style="text-align: center;">68.715</td>
    <td style="text-align: center;">73.345</td>
    <td style="text-align: center;">86.324</td>
    <td style="text-align: center;">2.096</td>
    <td style="text-align: center;">2.092</td>
    <td style="text-align: center;">2.106</td>
    <td style="text-align: center;">19.341</td>
  </tr>
  <tr>
    <td style="text-align: center;">51.758</td>
    <td style="text-align: center;">4.075</td>
    <td style="text-align: center;">549.637</td>
    <td style="text-align: center;">49.889</td>
    <td style="text-align: center;">68.267</td>
    <td style="text-align: center;">72.965</td>
    <td style="text-align: center;">86.32</td>
    <td style="text-align: center;">2.096</td>
    <td style="text-align: center;">2.092</td>
    <td style="text-align: center;">2.106</td>
    <td style="text-align: center;">19.321</td>
  </tr>
  <tr>
    <td style="text-align: center;">51.705</td>
    <td style="text-align: center;">4.045</td>
    <td style="text-align: center;">271.199</td>
    <td style="text-align: center;">48.598</td>
    <td style="text-align: center;">68.565</td>
    <td style="text-align: center;">73.179</td>
    <td style="text-align: center;">86.518</td>
    <td style="text-align: center;">2.096</td>
    <td style="text-align: center;">2.092</td>
    <td style="text-align: center;">2.1</td>
    <td style="text-align: center;">19.34</td>
  </tr>
  <tr>
    <td style="text-align: center;">51.891</td>
    <td style="text-align: center;">4.08</td>
    <td style="text-align: center;">406.665</td>
    <td style="text-align: center;">48.977</td>
    <td style="text-align: center;">68.61</td>
    <td style="text-align: center;">73.306</td>
    <td style="text-align: center;">86.515</td>
    <td style="text-align: center;">2.096</td>
    <td style="text-align: center;">2.092</td>
    <td style="text-align: center;">2.105</td>
    <td style="text-align: center;">19.272</td>
  </tr>
</table>
<table>
  <tr>
    <th colspan="11" style="text-align: center;">Read</th>
  </tr>
  <tr>
    <th colspan="7" style="text-align: center;">Response Time (ms)</th>
    <th colspan="3" style="text-align: center;">GC Alloc (mb)</th>
    <th colspan="1" style="text-align: center;">Throughput</th>
  </tr>
  <tr>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">Median</th>
    <th style="text-align: center;">90th pct</th>
    <th style="text-align: center;">95th pct</th>
    <th style="text-align: center;">99th pct</th>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">req/s</th>
  </tr>
  <tr>
    <td style="text-align: center;">1.266</td>
    <td style="text-align: center;">0.899</td>
    <td style="text-align: center;">315.608</td>
    <td style="text-align: center;">1.144</td>
    <td style="text-align: center;">1.414</td>
    <td style="text-align: center;">1.495</td>
    <td style="text-align: center;">1.716</td>
    <td style="text-align: center;">0.268</td>
    <td style="text-align: center;">0.254</td>
    <td style="text-align: center;">2.27</td>
    <td style="text-align: center;">790.183</td>
  </tr>
  <tr>
    <td style="text-align: center;">1.243</td>
    <td style="text-align: center;">0.9</td>
    <td style="text-align: center;">311.975</td>
    <td style="text-align: center;">1.144</td>
    <td style="text-align: center;">1.411</td>
    <td style="text-align: center;">1.486</td>
    <td style="text-align: center;">1.713</td>
    <td style="text-align: center;">0.268</td>
    <td style="text-align: center;">0.254</td>
    <td style="text-align: center;">2.27</td>
    <td style="text-align: center;">804.346</td>
  </tr>
  <tr>
    <td style="text-align: center;">1.221</td>
    <td style="text-align: center;">0.901</td>
    <td style="text-align: center;">309.656</td>
    <td style="text-align: center;">1.14</td>
    <td style="text-align: center;">1.402</td>
    <td style="text-align: center;">1.478</td>
    <td style="text-align: center;">1.703</td>
    <td style="text-align: center;">0.268</td>
    <td style="text-align: center;">0.254</td>
    <td style="text-align: center;">2.27</td>
    <td style="text-align: center;">818.731</td>
  </tr>
  <tr>
    <td style="text-align: center;">1.238</td>
    <td style="text-align: center;">0.894</td>
    <td style="text-align: center;">316.149</td>
    <td style="text-align: center;">1.129</td>
    <td style="text-align: center;">1.418</td>
    <td style="text-align: center;">1.498</td>
    <td style="text-align: center;">1.709</td>
    <td style="text-align: center;">0.268</td>
    <td style="text-align: center;">0.254</td>
    <td style="text-align: center;">2.27</td>
    <td style="text-align: center;">807.829</td>
  </tr>
  <tr>
    <td style="text-align: center;">1.292</td>
    <td style="text-align: center;">0.88</td>
    <td style="text-align: center;">316.703</td>
    <td style="text-align: center;">1.111</td>
    <td style="text-align: center;">1.408</td>
    <td style="text-align: center;">1.494</td>
    <td style="text-align: center;">1.754</td>
    <td style="text-align: center;">0.268</td>
    <td style="text-align: center;">0.254</td>
    <td style="text-align: center;">2.27</td>
    <td style="text-align: center;">774.264</td>
  </tr>
  <tr>
    <td style="text-align: center;">1.252</td>
    <td style="text-align: center;">0.895</td>
    <td style="text-align: center;">314.018</td>
    <td style="text-align: center;">1.133</td>
    <td style="text-align: center;">1.411</td>
    <td style="text-align: center;">1.49</td>
    <td style="text-align: center;">1.719</td>
    <td style="text-align: center;">0.268</td>
    <td style="text-align: center;">0.254</td>
    <td style="text-align: center;">2.27</td>
    <td style="text-align: center;">799.071</td>
  </tr>
</table>
<table>
  <tr>
    <th colspan="3" style="text-align: center;">ETC</th>
  </tr>
  <tr>
    <th colspan="1" style="text-align: center;">Processing Time (ms)</th>
    <th colspan="1" style="text-align: center;">GC Alloc (mb)</th>
    <th colspan="1" style="text-align: center;">Serialized Data Size (kb)</th>
  </tr>
  <tr>
    <th style="text-align: center;">Total</th>
    <th style="text-align: center;">Total</th>
    <th style="text-align: center;">Single</th>
  </tr>
  <tr>
    <td style="text-align: center;">1079965.203</td>
    <td style="text-align: center;">62289.757</td>
    <td style="text-align: center;">118.221</td>
  </tr>
  <tr>
    <td style="text-align: center;">1073038.901</td>
    <td style="text-align: center;">62289.618</td>
    <td style="text-align: center;">118.221</td>
  </tr>
  <tr>
    <td style="text-align: center;">1067087.028</td>
    <td style="text-align: center;">62287.887</td>
    <td style="text-align: center;">118.221</td>
  </tr>
  <tr>
    <td style="text-align: center;">1068426.936</td>
    <td style="text-align: center;">62289.032</td>
    <td style="text-align: center;">118.221</td>
  </tr>
  <tr>
    <td style="text-align: center;">1068439.566</td>
    <td style="text-align: center;">62286.656</td>
    <td style="text-align: center;">118.221</td>
  </tr>
  <tr>
    <td style="text-align: center;">1071391.527</td>
    <td style="text-align: center;">62288.59</td>
    <td style="text-align: center;">118.221</td>
  </tr>
</table>

### MemoryPack/MySQL
---

<table>
  <tr>
    <th colspan="6" style="text-align: center;">Serialize</th>
    <th colspan="6" style="text-align: center;">Deserialize</th>
  </tr>
  <tr>
    <th colspan="3" style="text-align: center;">ProcessingTime (ms)</th>
    <th colspan="3" style="text-align: center;">GC Alloc (mb)</th>
    <th colspan="3" style="text-align: center;">ProcessingTime (ms)</th>
    <th colspan="3" style="text-align: center;">GC Alloc (mb)</th>
  </tr>
  <tr>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
  </tr>
  <tr>
    <td style="text-align: center;">0.081</td>
    <td style="text-align: center;">0.061</td>
    <td style="text-align: center;">0.939</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">0.63</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">0.111</td>
    <td style="text-align: center;">0.811</td>
    <td style="text-align: center;">0.424</td>
    <td style="text-align: center;">0.418</td>
    <td style="text-align: center;">0.433</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.076</td>
    <td style="text-align: center;">0.057</td>
    <td style="text-align: center;">1.029</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">0.63</td>
    <td style="text-align: center;">0.133</td>
    <td style="text-align: center;">0.11</td>
    <td style="text-align: center;">0.641</td>
    <td style="text-align: center;">0.424</td>
    <td style="text-align: center;">0.418</td>
    <td style="text-align: center;">0.433</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.08</td>
    <td style="text-align: center;">0.061</td>
    <td style="text-align: center;">1.069</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">0.63</td>
    <td style="text-align: center;">0.133</td>
    <td style="text-align: center;">0.11</td>
    <td style="text-align: center;">0.667</td>
    <td style="text-align: center;">0.424</td>
    <td style="text-align: center;">0.418</td>
    <td style="text-align: center;">0.433</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.076</td>
    <td style="text-align: center;">0.056</td>
    <td style="text-align: center;">0.806</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">0.63</td>
    <td style="text-align: center;">0.132</td>
    <td style="text-align: center;">0.11</td>
    <td style="text-align: center;">0.738</td>
    <td style="text-align: center;">0.424</td>
    <td style="text-align: center;">0.418</td>
    <td style="text-align: center;">0.433</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.074</td>
    <td style="text-align: center;">0.057</td>
    <td style="text-align: center;">0.931</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">0.63</td>
    <td style="text-align: center;">0.132</td>
    <td style="text-align: center;">0.109</td>
    <td style="text-align: center;">0.673</td>
    <td style="text-align: center;">0.424</td>
    <td style="text-align: center;">0.418</td>
    <td style="text-align: center;">0.433</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.077</td>
    <td style="text-align: center;">0.058</td>
    <td style="text-align: center;">0.955</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">0.13</td>
    <td style="text-align: center;">0.63</td>
    <td style="text-align: center;">0.132</td>
    <td style="text-align: center;">0.11</td>
    <td style="text-align: center;">0.706</td>
    <td style="text-align: center;">0.424</td>
    <td style="text-align: center;">0.418</td>
    <td style="text-align: center;">0.433</td>
  </tr>
</table>
<table>
  <tr>
    <th colspan="11" style="text-align: center;">Write</th>
  </tr>
  <tr>
    <th colspan="7" style="text-align: center;">Response Time (ms)</th>
    <th colspan="3" style="text-align: center;">GC Alloc (mb)</th>
    <th colspan="1" style="text-align: center;">Throughput</th>
  </tr>
  <tr>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">Median</th>
    <th style="text-align: center;">90th pct</th>
    <th style="text-align: center;">95th pct</th>
    <th style="text-align: center;">99th pct</th>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">req/s</th>
  </tr>
  <tr>
    <td style="text-align: center;">53.83</td>
    <td style="text-align: center;">3.24</td>
    <td style="text-align: center;">312.874</td>
    <td style="text-align: center;">49.024</td>
    <td style="text-align: center;">69.657</td>
    <td style="text-align: center;">74.66</td>
    <td style="text-align: center;">89.587</td>
    <td style="text-align: center;">1.025</td>
    <td style="text-align: center;">1.022</td>
    <td style="text-align: center;">1.045</td>
    <td style="text-align: center;">18.577</td>
  </tr>
  <tr>
    <td style="text-align: center;">52.848</td>
    <td style="text-align: center;">3.268</td>
    <td style="text-align: center;">365.432</td>
    <td style="text-align: center;">49.63</td>
    <td style="text-align: center;">68.21</td>
    <td style="text-align: center;">72.976</td>
    <td style="text-align: center;">86.45</td>
    <td style="text-align: center;">1.025</td>
    <td style="text-align: center;">1.022</td>
    <td style="text-align: center;">1.045</td>
    <td style="text-align: center;">18.922</td>
  </tr>
  <tr>
    <td style="text-align: center;">53.058</td>
    <td style="text-align: center;">3.267</td>
    <td style="text-align: center;">386.684</td>
    <td style="text-align: center;">49.498</td>
    <td style="text-align: center;">69.217</td>
    <td style="text-align: center;">74.087</td>
    <td style="text-align: center;">87.473</td>
    <td style="text-align: center;">1.025</td>
    <td style="text-align: center;">1.022</td>
    <td style="text-align: center;">1.046</td>
    <td style="text-align: center;">18.847</td>
  </tr>
  <tr>
    <td style="text-align: center;">53.031</td>
    <td style="text-align: center;">3.251</td>
    <td style="text-align: center;">431.155</td>
    <td style="text-align: center;">49.189</td>
    <td style="text-align: center;">68.935</td>
    <td style="text-align: center;">73.928</td>
    <td style="text-align: center;">88.413</td>
    <td style="text-align: center;">1.025</td>
    <td style="text-align: center;">1.022</td>
    <td style="text-align: center;">1.044</td>
    <td style="text-align: center;">18.857</td>
  </tr>
  <tr>
    <td style="text-align: center;">52.33</td>
    <td style="text-align: center;">3.238</td>
    <td style="text-align: center;">563.982</td>
    <td style="text-align: center;">48.964</td>
    <td style="text-align: center;">67.843</td>
    <td style="text-align: center;">72.485</td>
    <td style="text-align: center;">85.391</td>
    <td style="text-align: center;">1.025</td>
    <td style="text-align: center;">1.022</td>
    <td style="text-align: center;">1.046</td>
    <td style="text-align: center;">19.109</td>
  </tr>
  <tr>
    <td style="text-align: center;">53.019</td>
    <td style="text-align: center;">3.253</td>
    <td style="text-align: center;">412.025</td>
    <td style="text-align: center;">49.261</td>
    <td style="text-align: center;">68.772</td>
    <td style="text-align: center;">73.627</td>
    <td style="text-align: center;">87.463</td>
    <td style="text-align: center;">1.025</td>
    <td style="text-align: center;">1.022</td>
    <td style="text-align: center;">1.045</td>
    <td style="text-align: center;">18.863</td>
  </tr>
</table>
<table>
  <tr>
    <th colspan="11" style="text-align: center;">Read</th>
  </tr>
  <tr>
    <th colspan="7" style="text-align: center;">Response Time (ms)</th>
    <th colspan="3" style="text-align: center;">GC Alloc (mb)</th>
    <th colspan="1" style="text-align: center;">Throughput</th>
  </tr>
  <tr>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">Median</th>
    <th style="text-align: center;">90th pct</th>
    <th style="text-align: center;">95th pct</th>
    <th style="text-align: center;">99th pct</th>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">req/s</th>
  </tr>
  <tr>
    <td style="text-align: center;">1.057</td>
    <td style="text-align: center;">0.723</td>
    <td style="text-align: center;">308.65</td>
    <td style="text-align: center;">0.994</td>
    <td style="text-align: center;">1.258</td>
    <td style="text-align: center;">1.361</td>
    <td style="text-align: center;">1.946</td>
    <td style="text-align: center;">0.156</td>
    <td style="text-align: center;">0.145</td>
    <td style="text-align: center;">2.161</td>
    <td style="text-align: center;">946.215</td>
  </tr>
  <tr>
    <td style="text-align: center;">1.123</td>
    <td style="text-align: center;">0.721</td>
    <td style="text-align: center;">316.681</td>
    <td style="text-align: center;">1.012</td>
    <td style="text-align: center;">1.26</td>
    <td style="text-align: center;">1.348</td>
    <td style="text-align: center;">1.862</td>
    <td style="text-align: center;">0.156</td>
    <td style="text-align: center;">0.145</td>
    <td style="text-align: center;">2.153</td>
    <td style="text-align: center;">890.727</td>
  </tr>
  <tr>
    <td style="text-align: center;">1.048</td>
    <td style="text-align: center;">0.744</td>
    <td style="text-align: center;">22.624</td>
    <td style="text-align: center;">1.016</td>
    <td style="text-align: center;">1.258</td>
    <td style="text-align: center;">1.35</td>
    <td style="text-align: center;">1.881</td>
    <td style="text-align: center;">0.156</td>
    <td style="text-align: center;">0.145</td>
    <td style="text-align: center;">2.161</td>
    <td style="text-align: center;">953.986</td>
  </tr>
  <tr>
    <td style="text-align: center;">1.096</td>
    <td style="text-align: center;">0.732</td>
    <td style="text-align: center;">311.165</td>
    <td style="text-align: center;">1.015</td>
    <td style="text-align: center;">1.285</td>
    <td style="text-align: center;">1.373</td>
    <td style="text-align: center;">1.856</td>
    <td style="text-align: center;">0.156</td>
    <td style="text-align: center;">0.145</td>
    <td style="text-align: center;">2.161</td>
    <td style="text-align: center;">912.648</td>
  </tr>
  <tr>
    <td style="text-align: center;">1.118</td>
    <td style="text-align: center;">0.726</td>
    <td style="text-align: center;">315.935</td>
    <td style="text-align: center;">1.005</td>
    <td style="text-align: center;">1.267</td>
    <td style="text-align: center;">1.355</td>
    <td style="text-align: center;">1.899</td>
    <td style="text-align: center;">0.156</td>
    <td style="text-align: center;">0.145</td>
    <td style="text-align: center;">2.161</td>
    <td style="text-align: center;">894.839</td>
  </tr>
  <tr>
    <td style="text-align: center;">1.088</td>
    <td style="text-align: center;">0.729</td>
    <td style="text-align: center;">255.011</td>
    <td style="text-align: center;">1.008</td>
    <td style="text-align: center;">1.266</td>
    <td style="text-align: center;">1.358</td>
    <td style="text-align: center;">1.889</td>
    <td style="text-align: center;">0.156</td>
    <td style="text-align: center;">0.145</td>
    <td style="text-align: center;">2.16</td>
    <td style="text-align: center;">919.683</td>
  </tr>
</table>
<table>
  <tr>
    <th colspan="3" style="text-align: center;">ETC</th>
  </tr>
  <tr>
    <th colspan="1" style="text-align: center;">Processing Time (ms)</th>
    <th colspan="1" style="text-align: center;">GC Alloc (mb)</th>
    <th colspan="1" style="text-align: center;">Serialized Data Size (kb)</th>
  </tr>
  <tr>
    <th style="text-align: center;">Total</th>
    <th style="text-align: center;">Total</th>
    <th style="text-align: center;">Single</th>
  </tr>
  <tr>
    <td style="text-align: center;">1101944.244</td>
    <td style="text-align: center;">34704.194</td>
    <td style="text-align: center;">132.892</td>
  </tr>
  <tr>
    <td style="text-align: center;">1083598.448</td>
    <td style="text-align: center;">34703.581</td>
    <td style="text-align: center;">132.892</td>
  </tr>
  <tr>
    <td style="text-align: center;">1086390.794</td>
    <td style="text-align: center;">34703.622</td>
    <td style="text-align: center;">132.892</td>
  </tr>
  <tr>
    <td style="text-align: center;">1086689.543</td>
    <td style="text-align: center;">34702.913</td>
    <td style="text-align: center;">132.892</td>
  </tr>
  <tr>
    <td style="text-align: center;">1073078.394</td>
    <td style="text-align: center;">34703.28</td>
    <td style="text-align: center;">132.892</td>
  </tr>
  <tr>
    <td style="text-align: center;">1086340.285</td>
    <td style="text-align: center;">34703.518</td>
    <td style="text-align: center;">132.892</td>
  </tr>
</table>

### BSON/MongoDB
---

<table>
  <tr>
    <th colspan="6" style="text-align: center;">Serialize</th>
    <th colspan="6" style="text-align: center;">Deserialize</th>
  </tr>
  <tr>
    <th colspan="3" style="text-align: center;">ProcessingTime (ms)</th>
    <th colspan="3" style="text-align: center;">GC Alloc (mb)</th>
    <th colspan="3" style="text-align: center;">ProcessingTime (ms)</th>
    <th colspan="3" style="text-align: center;">GC Alloc (mb)</th>
  </tr>
  <tr>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
  </tr>
  <tr>
    <td style="text-align: center;">0.327</td>
    <td style="text-align: center;">0.268</td>
    <td style="text-align: center;">2.143</td>
    <td style="text-align: center;">0.707</td>
    <td style="text-align: center;">0.699</td>
    <td style="text-align: center;">1.212</td>
    <td style="text-align: center;">0.381</td>
    <td style="text-align: center;">0.321</td>
    <td style="text-align: center;">1.89</td>
    <td style="text-align: center;">0.724</td>
    <td style="text-align: center;">0.718</td>
    <td style="text-align: center;">0.742</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.316</td>
    <td style="text-align: center;">0.265</td>
    <td style="text-align: center;">2.973</td>
    <td style="text-align: center;">0.707</td>
    <td style="text-align: center;">0.699</td>
    <td style="text-align: center;">1.212</td>
    <td style="text-align: center;">0.387</td>
    <td style="text-align: center;">0.332</td>
    <td style="text-align: center;">2.166</td>
    <td style="text-align: center;">0.724</td>
    <td style="text-align: center;">0.718</td>
    <td style="text-align: center;">0.746</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.31</td>
    <td style="text-align: center;">0.267</td>
    <td style="text-align: center;">2.199</td>
    <td style="text-align: center;">0.707</td>
    <td style="text-align: center;">0.699</td>
    <td style="text-align: center;">1.204</td>
    <td style="text-align: center;">0.358</td>
    <td style="text-align: center;">0.303</td>
    <td style="text-align: center;">2.326</td>
    <td style="text-align: center;">0.724</td>
    <td style="text-align: center;">0.718</td>
    <td style="text-align: center;">0.739</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.314</td>
    <td style="text-align: center;">0.264</td>
    <td style="text-align: center;">2.845</td>
    <td style="text-align: center;">0.707</td>
    <td style="text-align: center;">0.699</td>
    <td style="text-align: center;">1.209</td>
    <td style="text-align: center;">0.359</td>
    <td style="text-align: center;">0.304</td>
    <td style="text-align: center;">2.098</td>
    <td style="text-align: center;">0.724</td>
    <td style="text-align: center;">0.718</td>
    <td style="text-align: center;">0.738</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.321</td>
    <td style="text-align: center;">0.269</td>
    <td style="text-align: center;">3.15</td>
    <td style="text-align: center;">0.707</td>
    <td style="text-align: center;">0.699</td>
    <td style="text-align: center;">1.212</td>
    <td style="text-align: center;">0.387</td>
    <td style="text-align: center;">0.332</td>
    <td style="text-align: center;">2.216</td>
    <td style="text-align: center;">0.724</td>
    <td style="text-align: center;">0.718</td>
    <td style="text-align: center;">0.77</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.318</td>
    <td style="text-align: center;">0.267</td>
    <td style="text-align: center;">2.662</td>
    <td style="text-align: center;">0.707</td>
    <td style="text-align: center;">0.699</td>
    <td style="text-align: center;">1.21</td>
    <td style="text-align: center;">0.375</td>
    <td style="text-align: center;">0.318</td>
    <td style="text-align: center;">2.139</td>
    <td style="text-align: center;">0.724</td>
    <td style="text-align: center;">0.718</td>
    <td style="text-align: center;">0.747</td>
  </tr>
</table>
<table>
  <tr>
    <th colspan="11" style="text-align: center;">Write</th>
  </tr>
  <tr>
    <th colspan="7" style="text-align: center;">Response Time (ms)</th>
    <th colspan="3" style="text-align: center;">GC Alloc (mb)</th>
    <th colspan="1" style="text-align: center;">Throughput</th>
  </tr>
  <tr>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">Median</th>
    <th style="text-align: center;">90th pct</th>
    <th style="text-align: center;">95th pct</th>
    <th style="text-align: center;">99th pct</th>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">req/s</th>
  </tr>
  <tr>
    <td style="text-align: center;">2.912</td>
    <td style="text-align: center;">0.648</td>
    <td style="text-align: center;">93.116</td>
    <td style="text-align: center;">1.103</td>
    <td style="text-align: center;">1.516</td>
    <td style="text-align: center;">2.086</td>
    <td style="text-align: center;">49.361</td>
    <td style="text-align: center;">0.061</td>
    <td style="text-align: center;">0.055</td>
    <td style="text-align: center;">0.094</td>
    <td style="text-align: center;">343.408</td>
  </tr>
  <tr>
    <td style="text-align: center;">2.558</td>
    <td style="text-align: center;">0.637</td>
    <td style="text-align: center;">63.323</td>
    <td style="text-align: center;">1.082</td>
    <td style="text-align: center;">1.47</td>
    <td style="text-align: center;">1.94</td>
    <td style="text-align: center;">48.998</td>
    <td style="text-align: center;">0.061</td>
    <td style="text-align: center;">0.055</td>
    <td style="text-align: center;">0.094</td>
    <td style="text-align: center;">390.892</td>
  </tr>
  <tr>
    <td style="text-align: center;">2.461</td>
    <td style="text-align: center;">0.63</td>
    <td style="text-align: center;">339.317</td>
    <td style="text-align: center;">1.089</td>
    <td style="text-align: center;">1.432</td>
    <td style="text-align: center;">1.741</td>
    <td style="text-align: center;">48.648</td>
    <td style="text-align: center;">0.061</td>
    <td style="text-align: center;">0.055</td>
    <td style="text-align: center;">0.11</td>
    <td style="text-align: center;">406.355</td>
  </tr>
  <tr>
    <td style="text-align: center;">2.616</td>
    <td style="text-align: center;">0.648</td>
    <td style="text-align: center;">59.843</td>
    <td style="text-align: center;">1.096</td>
    <td style="text-align: center;">1.452</td>
    <td style="text-align: center;">1.943</td>
    <td style="text-align: center;">48.836</td>
    <td style="text-align: center;">0.061</td>
    <td style="text-align: center;">0.055</td>
    <td style="text-align: center;">0.11</td>
    <td style="text-align: center;">382.27</td>
  </tr>
  <tr>
    <td style="text-align: center;">2.572</td>
    <td style="text-align: center;">0.64</td>
    <td style="text-align: center;">68.337</td>
    <td style="text-align: center;">1.088</td>
    <td style="text-align: center;">1.474</td>
    <td style="text-align: center;">2.036</td>
    <td style="text-align: center;">48.713</td>
    <td style="text-align: center;">0.061</td>
    <td style="text-align: center;">0.055</td>
    <td style="text-align: center;">0.094</td>
    <td style="text-align: center;">388.871</td>
  </tr>
  <tr>
    <td style="text-align: center;">2.624</td>
    <td style="text-align: center;">0.641</td>
    <td style="text-align: center;">124.787</td>
    <td style="text-align: center;">1.091</td>
    <td style="text-align: center;">1.469</td>
    <td style="text-align: center;">1.949</td>
    <td style="text-align: center;">48.911</td>
    <td style="text-align: center;">0.061</td>
    <td style="text-align: center;">0.055</td>
    <td style="text-align: center;">0.1</td>
    <td style="text-align: center;">382.359</td>
  </tr>
</table>
<table>
  <tr>
    <th colspan="11" style="text-align: center;">Read</th>
  </tr>
  <tr>
    <th colspan="7" style="text-align: center;">Response Time (ms)</th>
    <th colspan="3" style="text-align: center;">GC Alloc (mb)</th>
    <th colspan="1" style="text-align: center;">Throughput</th>
  </tr>
  <tr>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">Median</th>
    <th style="text-align: center;">90th pct</th>
    <th style="text-align: center;">95th pct</th>
    <th style="text-align: center;">99th pct</th>
    <th style="text-align: center;">Average</th>
    <th style="text-align: center;">Min</th>
    <th style="text-align: center;">Max</th>
    <th style="text-align: center;">req/s</th>
  </tr>
  <tr>
    <td style="text-align: center;">1.023</td>
    <td style="text-align: center;">0.718</td>
    <td style="text-align: center;">438.968</td>
    <td style="text-align: center;">0.884</td>
    <td style="text-align: center;">1.109</td>
    <td style="text-align: center;">1.335</td>
    <td style="text-align: center;">1.604</td>
    <td style="text-align: center;">0.491</td>
    <td style="text-align: center;">0.478</td>
    <td style="text-align: center;">2.492</td>
    <td style="text-align: center;">977.699</td>
  </tr>
  <tr>
    <td style="text-align: center;">0.985</td>
    <td style="text-align: center;">0.707</td>
    <td style="text-align: center;">269.44</td>
    <td style="text-align: center;">0.869</td>
    <td style="text-align: center;">1.027</td>
    <td style="text-align: center;">1.124</td>
    <td style="text-align: center;">1.554</td>
    <td style="text-align: center;">0.491</td>
    <td style="text-align: center;">0.482</td>
    <td style="text-align: center;">2.492</td>
    <td style="text-align: center;">1015.362</td>
  </tr>
  <tr>
    <td style="text-align: center;">1.007</td>
    <td style="text-align: center;">0.734</td>
    <td style="text-align: center;">26.107</td>
    <td style="text-align: center;">0.916</td>
    <td style="text-align: center;">1.08</td>
    <td style="text-align: center;">1.315</td>
    <td style="text-align: center;">1.613</td>
    <td style="text-align: center;">0.491</td>
    <td style="text-align: center;">0.481</td>
    <td style="text-align: center;">2.484</td>
    <td style="text-align: center;">992.709</td>
  </tr>
  <tr>
    <td style="text-align: center;">1.015</td>
    <td style="text-align: center;">0.725</td>
    <td style="text-align: center;">351.335</td>
    <td style="text-align: center;">0.909</td>
    <td style="text-align: center;">1.122</td>
    <td style="text-align: center;">1.37</td>
    <td style="text-align: center;">1.57</td>
    <td style="text-align: center;">0.491</td>
    <td style="text-align: center;">0.481</td>
    <td style="text-align: center;">2.484</td>
    <td style="text-align: center;">984.838</td>
  </tr>
  <tr>
    <td style="text-align: center;">1.017</td>
    <td style="text-align: center;">0.73</td>
    <td style="text-align: center;">165.686</td>
    <td style="text-align: center;">0.9</td>
    <td style="text-align: center;">1.1</td>
    <td style="text-align: center;">1.36</td>
    <td style="text-align: center;">1.772</td>
    <td style="text-align: center;">0.491</td>
    <td style="text-align: center;">0.481</td>
    <td style="text-align: center;">2.484</td>
    <td style="text-align: center;">982.815</td>
  </tr>
  <tr>
    <td style="text-align: center;">1.01</td>
    <td style="text-align: center;">0.723</td>
    <td style="text-align: center;">250.307</td>
    <td style="text-align: center;">0.896</td>
    <td style="text-align: center;">1.088</td>
    <td style="text-align: center;">1.301</td>
    <td style="text-align: center;">1.623</td>
    <td style="text-align: center;">0.491</td>
    <td style="text-align: center;">0.481</td>
    <td style="text-align: center;">2.488</td>
    <td style="text-align: center;">990.684</td>
  </tr>
</table>
<table>
  <tr>
    <th colspan="3" style="text-align: center;">ETC</th>
  </tr>
  <tr>
    <th colspan="1" style="text-align: center;">Processing Time (ms)</th>
    <th colspan="1" style="text-align: center;">GC Alloc (mb)</th>
    <th colspan="1" style="text-align: center;">Serialized Data Size (kb)</th>
  </tr>
  <tr>
    <th style="text-align: center;">Total</th>
    <th style="text-align: center;">Total</th>
    <th style="text-align: center;">Single</th>
  </tr>
  <tr>
    <td style="text-align: center;">92847.74</td>
    <td style="text-align: center;">39658.189</td>
    <td style="text-align: center;">134.695</td>
  </tr>
  <tr>
    <td style="text-align: center;">84939.829</td>
    <td style="text-align: center;">39655.976</td>
    <td style="text-align: center;">134.695</td>
  </tr>
  <tr>
    <td style="text-align: center;">82729.852</td>
    <td style="text-align: center;">39656.343</td>
    <td style="text-align: center;">134.695</td>
  </tr>
  <tr>
    <td style="text-align: center;">86101.533</td>
    <td style="text-align: center;">39656.194</td>
    <td style="text-align: center;">134.695</td>
  </tr>
  <tr>
    <td style="text-align: center;">85954.516</td>
    <td style="text-align: center;">39657.035</td>
    <td style="text-align: center;">134.695</td>
  </tr>
  <tr>
    <td style="text-align: center;">86514.694</td>
    <td style="text-align: center;">39656.747</td>
    <td style="text-align: center;">134.695</td>
  </tr>
</table>

---

</div>