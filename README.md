# Orleans-Sample
#EventSourcing, #DDD #dotnetcore #eventStore  

Orleans Sample DDD, Event sourcing using Orleans and Event Store

to use this sample:

- run event store using docker: refer to https://hub.docker.com/r/eventstore/eventstore/
- update eventstore uri environment variable in &quot;Godwit.Silo&quot; launch settings json file
- run &quot;Godwit.Silo&quot; Project
- Run &quot;Godwit.WebApi&quot; Project

to verify the usage

- using &quot;Postman&quot;, load the collection &quot;[https://www.getpostman.com/collections/174ad0d025a5b17b30af](https://www.getpostman.com/collections/174ad0d025a5b17b30af)&quot;
