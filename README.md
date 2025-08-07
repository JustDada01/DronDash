# DronDash
DroneDash is a console application written in C# that simulates a food delivery system from restaurants to customers using drones.

The application allows you to:

    Generate orders (random city, street, customer from a list).

    View the full list of orders.

    Manage the fleet of drones (add new drones, assign them to orders, change their statuses, list all drones).

    Change the status of an order (New → In Delivery → Completed or Rejected).

Assumptions:

    Each drone can handle only one order at a time.

    After an order is completed or rejected, the drone returns to the Inactive state.

    Orders and drones are stored only in memory (in a list within a single program session).

    Console interface: main menu with a loop, user input is validated, and selecting 0 always cancels the current action and returns to the previous menu level.
