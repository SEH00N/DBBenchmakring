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
|Processing Time Max        |Response Time Max          |                           |
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
|![](.\Report\Colors\JSON_Redis.png)  |![](.\Report\Colors\MemoryPack_Redis.png)  |![](.\Report\Colors\JSON_MySQL.png)  |![](.\Report\Colors\MemoryPack_MySQL.png)  |![](.\Report\Colors\BSON_MongoDB.png)  |

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

### Detail

[JSON/Redis](.\Report\Statistics\JSON_Redis.png)

[MemoryPack/Redis](.\Report\Statistics\MemoryPack_Redis.png)

[JSON/MySQL](.\Report\Statistics\JSON_MySQL.png)

[MemoryPack/MySQL](.\Report\Statistics\MemoryPack_MySQL.png)

[BSON/MongoDB](.\Report\Statistics\BSON_MongoDB.png)

</div>