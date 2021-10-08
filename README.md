# BikeShop
A thrown away blazor wasm client for the bikeDistributor library

This is actually my first try at building an wasm blazor app. 
I was a bit skeptical at start, I expected a similar experience as building webapp with old days webforms. 
In fact the programming model is very much the same but a lot smoother. 
You get a strong Déjà vu  feeling and start thinking that "...this is how webforms should have been back in the days..."

Blazor is very young and I don't think it has a very large pool of potential users besides those developers 
who are not in love with javascript.

As far as I tested (which is not much) I noticed:
- project folder structure is clear and very customisable
- routing is a breeze. You just assign url directly in the blazor partial
- templating and site wire-framing is truly quick
- services registration and DI are very similar to an aspnetcoreMVC project but a bit more messy 

got service registration working but ran into....

## my first big gotcha ##
After a a day of unsuccesful trying to connect to a MongoDb server I fully understood the implications of this sentence on Microsoft docs
`
...code is executing in the browser sandbox ....
`
this applies only to blazor wasm (not blazor server) but it means http is the only protocol that works which in turn means 
the whole System.Net namespace (with the exception of System.Net.http) is not supported which in turn means no direct connection to Databases or sending mails unless I 
put up a webservice backend. That makes wasm not exactly the perfect candidate for testing the portability of my library...
Next time I'll make sure to better read the docs


(...more to come...)
