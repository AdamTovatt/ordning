## **Must have**

* Locations as the core concept for organizing items
* Unique, human-readable string ID for a location
* Required name and optional description for locations
* Items with a required name and optional description
* Easy way to add, remove, and move items
* Move a single item between locations
* Items can be stored in a location
* UTC timestamps (Postgres `timestamptz`) on locations and items
* Easy-to-use UI for:
  * managing locations
  * placing items into locations

---

## **Should have**

* Move multiple items at once (bulk move)
* Items can be split across multiple locations
* Optional item properties (key/value stored as strings)
* Protection against duplicate location IDs
* Search by name and location
* Separate history table for item movements (with timestamps)

---

## **Could have**

* Filtering by properties
* Display history in the UI (what / when)
* Export/import (CSV/JSON)
* Mobile-friendly UI
* Preparation for relationships between locations (hierarchy, tags)

---

## **Won’t have (for now)**

* Images for items
* Advanced location hierarchy
* Roles/permissions
* Automations or smart suggestions
* Barcodes / QR codes

---