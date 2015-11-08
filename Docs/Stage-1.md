Backend API for mobile app
===

**Points**: *15 / 50*

The task is to create a mobile app with server back-end for managing user's notes regarding grocery products. User should be able to add products that he or she is interested in monitoring. When a new product is added, it is stored on the server with amount equal to 0 (zero). For each existing product three operations should be available:

* increase amount - invoked when the user buys additional items of the given product,
* decrease amount - invoked when some items of the product are consumed,
* remove - invoked when the user is no longer interested in monitoring the amount of a given product.

For increase amount and decrease amount operations the user should be able to input the quantity of bought/consumed product.

Assuming above set of operations, the app can be used to keep track of the number of grocery products stored at home. When shopping this data can be utilized to decide which products have to be acquired so that the user won't run out of stock.

The list of products should be stored on the server. It is not required at this stage of the project to store the data on the mobile device (this ability will be added in stage II). All operations on products should immediately call appropriate server API methods, to update the state on the server.

The application should handle multiple users. Authentication may be based on login and password credentials or social accounts integration (e.g. signing up with Google account, Facebook account etc).

There are no restrictions regarding technologies used for the mobile app and the server side app - use the ones you see fit best.
