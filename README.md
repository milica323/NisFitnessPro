Projekat se sastoji od .NET 8 Web API-ja i Redisa.
docker run -d --name redis-fitness -p 6379:6379 redis

Šta je implementirano (Ukratko)
U okviru servisa su demonstrirane sledeće Redis funkcionalnosti:

ZSET: Za rang listu trkača (automatsko sortiranje).

GEO: Za "Radar" pretragu trkača u radijusu od 5km oko Niša.

LIST: Za Activity Feed (sa LTRIM ograničenjem na 10 unosa).

Distributed Lock: Za bezbedno ažuriranje globalnog rekorda bez kolizija.

Pub/Sub: Sistem za obaveštavanje o novim rekordima.
