

## MY_Prod

<!-- Redis -->
<add name="Prod.Redis.Cache" connectionString="backend-redis.my.91app.io:6379,ssl=false,password=,allowAdmin=false,connectTimeout=5000" />
<add name="Prod.Redis.Data.Frontend" connectionString="data-cache.my.91app.io:6379,ssl=false,password=,allowAdmin=false,connectTimeout=5000" />
<add name="Prod.Redis.Cache.Frontend" connectionString="data-cache.my.91app.io:6379,ssl=false,password=,allowAdmin=false,connectTimeout=5000" />
<add name="Prod.Redis.ImageQueue" connectionString="data-cache.my.91app.io:6379,ssl=false,password=,allowAdmin=false,connectTimeout=5000" />


## HK_Prod

<add name="Prod.Redis.Cache" connectionString="backend-redis.hk.91app.io:6379,ssl=false,password=,allowAdmin=false,connectTimeout=5000,syncTimeout=3000"/>
<add name="Prod.Redis.Cache.Frontend" connectionString="cache-redis.hk.91app.io:6379,ssl=false,password=,allowAdmin=false,connectTimeout=5000,syncTimeout=3000"/>
<add name="Prod.Redis.Data.Frontend" connectionString="data-redis.hk.91app.io:6379,ssl=false,password=,allowAdmin=false,connectTimeout=5000,syncTimeout=3000"/>
<add name="Prod.Redis.ImageQueue" connectionString="cache-redis.hk.91app.io:6379,abortConnect=false,ssl=false,password=,allowAdmin=false,connectTimeout=5000"/>



 