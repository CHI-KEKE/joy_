
## 看 mweb 吃的 outputCache domain

MobileWebMall 的 OutputCache 設定檔參考連結

https://bitbucket.org/nineyi/nineyi.configuration/src/master/ApplicationConfig/NineYi.WebStore.MobileWebMall/WebAPI/OutputCacheSettings.Prod.MY.config


## HK Prod keys
<add name="Prod.Redis.Cache" connectionString="cache-redis.hk.91app.io:6379,ssl=false,password=,allowAdmin=false,connectTimeout=5000,syncTimeout=2000"/>
<add name="Prod.Redis.Data" connectionString="data-redis.hk.91app.io:6379,ssl=false,password=,allowAdmin=false,connectTimeout=5000,syncTimeout=2000"/>
<add name="Prod.Redis.Data2" connectionString="data-redis.hk.91app.io:6379,ssl=false,password=,allowAdmin=false,connectTimeout=5000,syncTimeout=2000"/>
<add name="Prod.Redis.ImageQueue" connectionString="cache-redis.hk.91app.io:6379,abortConnect=false,ssl=false,password=,allowAdmin=false,connectTimeout=5000,syncTimeout=2000"/>
<add name="Prod.Redis.LineUp" connectionString="cache-redis.hk.91app.io:6379,ssl=false,password=,allowAdmin=false,connectTimeout=5000,syncTimeout=2000"/>
<add name="Prod.Redis.FileCache" connectionString="cache-redis.hk.91app.io:6379,ssl=false,password=,allowAdmin=false,connectTimeout=5000,syncTimeout=2000"/>
<add name="Prod.Redis.Sequence" connectionString="sequence-cache.hk.91app.io:6379,ssl=false,password=,allowAdmin=false,connectTimeout=5000,syncTimeout=2000"/>

<br>

## HK QA keys

<add name="QA.Redis.Cache" connectionString="10.51.106.123:6379,ssl=false,password=,allowAdmin=false,connectTimeout=50000"/>
<add name="QA.Redis.Data" connectionString="10.51.106.123:6379,ssl=false,password=,allowAdmin=false,connectTimeout=50000"/>
<add name="QA.Redis.ImageQueue" connectionString="10.51.106.123:6379,abortConnect=false,ssl=false,password=,allowAdmin=false,connectTimeout=5000,syncTimeout=2000"/>
<add name="QA.Redis.Data2" connectionString="10.51.106.123:6379,ssl=false,password=,allowAdmin=false,connectTimeout=50000"/>
<add name="QA.Redis.LineUp" connectionString="10.51.106.123:6379,ssl=false,password=,allowAdmin=false,connectTimeout=5000"/>
<add name="QA.Redis.FileCache" connectionString="10.50.12.118:6379,ssl=false,password=,allowAdmin=false,connectTimeout=5000"/>
<add name="QA.Redis.Sequence" connectionString="10.51.106.123:6379,ssl=false,password=,allowAdmin=false,connectTimeout=5000"/>

<br>

## MY Prods

<!-- Redis -->
<add name="Prod.Redis.Cache" connectionString="data-cache2.my.91app.io:6379,ssl=false,password=,allowAdmin=false,connectTimeout=5000,syncTimeout=2000"/>
<add name="Prod.Redis.Data" connectionString="data-cache2.my.91app.io:6379,ssl=false,password=,allowAdmin=false,connectTimeout=5000,syncTimeout=2000"/>
<add name="Prod.Redis.ImageQueue" connectionString="data-cache2.my.91app.io:6379,abortConnect=false,ssl=false,password=,allowAdmin=false,connectTimeout=5000,syncTimeout=2000"/>
<add name="Prod.Redis.Data2" connectionString="data-cache2.my.91app.io:6379,ssl=false,password=,allowAdmin=false,connectTimeout=5000,syncTimeout=2000"/>
<add name="Prod.Redis.LineUp" connectionString="data-cache2.my.91app.io:6379,ssl=false,password=,allowAdmin=false,connectTimeout=5000,syncTimeout=2000"/>
<add name="Prod.Redis.FileCache" connectionString="data-cache2.my.91app.io:6379,ssl=false,password=,allowAdmin=false,connectTimeout=5000,syncTimeout=2000"/>
<add name="Prod.Redis.Sequence" connectionString="sequence-cache.my.91app.io:6379,ssl=false,password=,allowAdmin=false,connectTimeout=5000,syncTimeout=2000"/>
<

## MY QA

<!-- Redis -->
<add name="QA.Redis.Cache" connectionString="10.51.121.152:6379,ssl=false,password=,allowAdmin=false,connectTimeout=50000"/>
<add name="QA.Redis.Data" connectionString="10.51.121.152:6379,ssl=false,password=,allowAdmin=false,connectTimeout=50000"/>
<add name="QA.Redis.ImageQueue" connectionString="10.51.121.152:6379,abortConnect=false,ssl=false,password=,allowAdmin=false,connectTimeout=5000,syncTimeout=2000"/>
<add name="QA.Redis.Data2" connectionString="10.51.121.152:6379,ssl=false,password=,allowAdmin=false,connectTimeout=50000"/>
<add name="QA.Redis.LineUp" connectionString="10.51.121.152:6379,ssl=false,password=,allowAdmin=false,connectTimeout=5000"/>
<add name="QA.Redis.FileCache" connectionString="10.51.121.152:6379,ssl=false,password=,allowAdmin=false,connectTimeout=50000"/>
<add name="QA.Redis.Sequence" connectionString="10.51.121.152:6379,ssl=false,password=,allowAdmin=false,connectTimeout=5000"/>
