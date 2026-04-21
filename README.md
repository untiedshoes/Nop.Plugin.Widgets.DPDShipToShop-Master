# DPD Ship to Shop for nopCommerce 4.50

A self-contained nopCommerce widget plugin that integrates the DPD pickup-point API into checkout and order management.

This project is designed for **nopCommerce 4.50** and keeps the integration inside the plugin so the main nopCommerce source does not need to be modified.

## Overview

The plugin allows customers to select a DPD pickup shop during checkout instead of using a home-delivery address. It retrieves nearby pickup locations from the DPD API, displays them on a Google Map, and stores the selected location against the customer and order.

It also adds an admin-side DPD shipment screen and a custom order token so the chosen pickup location can be shown in emails and order notes.

## Key features

- DPD pickup-point lookup during checkout
- Interactive Google Maps display of nearby DPD shops
- Postcode-based search with optional Google autocomplete
- Storage of the chosen pickup location against the order
- Automatic order note containing the selected DPD location
- Custom message token for order emails
- Admin configuration page inside nopCommerce
- Admin order button for DPD shipment preparation
- Fully isolated plugin project for nopCommerce 4.50

## Compatibility

- **Plugin type:** Widget plugin
- **System name:** `Widgets.DPDShipToShop`
- **Version:** `1.20`
- **Supported nopCommerce version:** `4.50`
- **Target framework:** `.NET 6`

## Tech stack

This plugin is built with the following core technologies:

- **C#** for the plugin implementation
- **.NET 6** as the runtime target
- **ASP.NET Core MVC** for controllers and Razor rendering
- **nopCommerce 4.50** plugin and widget infrastructure
- **JavaScript, jQuery, and AJAX requests** for the checkout map interactions
- **Google Maps JavaScript API** and optional Places autocomplete
- **DPD REST API** for login, pickup-point lookup, and shipment operations
- **Newtonsoft.Json** for JSON serialization and deserialization
- **nopCommerce data/repository services** for storing selected pickup-point information

## Architecture overview

The plugin follows a layered nopCommerce plugin structure:

### 1. Presentation layer

Responsible for what store customers and admins see:

- `Components/` provides the checkout map widget and the admin shipment button
- `Views/` contains the public checkout UI and admin configuration pages
- `Scripts/` contains the Google Maps and pickup-point interaction logic
- `Themes/` contains styles and map marker assets

### 2. Controller layer

Coordinates requests between the UI, the DPD API, and stored plugin data:

- configuration management
- pickup-point search endpoints
- pickup-location selection handling
- admin shipment page routing

### 3. Integration layer

Handles communication with DPD:

- `DPDShipToShopClient.cs` sends login, pickup-point, and shipment requests once headers are prepared
- `Services/DPDSessionService.cs` centrally manages DPD `geoSession` reuse and refresh
- `Request/` and `Response/` define the request and response models
- `ServiceModelConfig.cs` prepares the API communication setup

### 4. Domain and persistence layer

Stores selected DPD pickup-point data for customers and orders:

- `Domain/` defines the data entities
- `Data/` contains schema and mapping logic
- `Services/` contains business logic and repository-based operations

### 5. Event and messaging layer

Extends nopCommerce order processing:

- order placed events save the chosen pickup location against the order
- a custom message token makes the pickup-point address available in emails

Overall flow:

1. The checkout widget appears for the configured shipping method.
2. The plugin gets a valid cached DPD `geoSession`, refreshing it only when needed.
3. The plugin retrieves nearby pickup locations from DPD.
4. The customer selects a location on the map.
5. The selection is stored and later attached to the order.
6. Admin users can review the order and open the DPD shipment page.

## How the plugin works

1. The customer reaches the shipping method step in checkout.
2. When the selected shipping method matches the configured **Shipping Method Name**, the DPD pickup-point widget is shown.
3. The plugin asks the session service for a valid DPD `geoSession`.
4. If a valid session is already cached, it is reused instead of logging in again.
5. If there is no cached session or it has expired, the plugin performs a fresh DPD login and stores the new `geoSession` in distributed cache.
6. The plugin searches for nearby pickup shops using the customer postcode and country code.
7. The customer selects a pickup location from the map or ranked list.
8. The chosen shop is saved and later attached to the order as an order note and email token content.

## Checkout experience

The checkout widget is rendered in the nopCommerce widget zone:

- `OpCheckoutShippingMethodBottom`

The customer can:

- search by postcode
- see nearby pickup points on a map
- review opening times
- view location details and accessibility information
- select the preferred DPD shop before continuing

## Admin experience

The plugin adds:

- a configuration page at **Admin → DPD Ship to Shop → Configure**
- a button on the order details page for eligible DPD orders
- a DPD-oriented shipment form with fields such as:
  - collection date
  - number of parcels
  - parcel weight
  - delivery instructions
  - parcel description

The admin order button is shown for paid orders using the shipping method **DPD Ship to Shop**.

## Required services and dependencies

To use the plugin you will need:

- a working nopCommerce 4.50 installation
- valid DPD API credentials
- a DPD account number
- the correct DPD API base URL for your account
- a Google Maps API key
- a shipping option in nopCommerce whose display name matches the configured shipping method name

## Installation

### Option 1: Build inside a nopCommerce 4.50 solution

1. Place the project in your nopCommerce plugins folder.
2. Build the solution.
3. The plugin output is copied to:
   - `Presentation/Nop.Web/Plugins/Widgets.DPDShipToShop`
4. Restart the site if needed.
5. Install or reload the plugin from the nopCommerce admin area.

### Option 2: Deploy compiled output

Copy the compiled plugin files into the nopCommerce plugins directory for:

- `Widgets.DPDShipToShop`

Then restart the application and install the plugin from admin.

## Configuration

After installation, open the plugin configuration page and set the following values:

| Setting | Purpose |
|---|---|
| `PluginEnabled` | Enables or disables the plugin |
| `UserName` | DPD API username |
| `Password` | DPD API password |
| `AccountNumber` | DPD account number used in API headers |
| `DPDBaseURL` | Base URL for the DPD API |
| `ShippingMethodName` | Exact shipping method name that should trigger the widget |
| `GoogleMapsApiKey` | Google Maps API key used to display the map |
| `UseGoogleAutoComplete` | Enables Google Places autocomplete for location search |
| `StoreUrl` | Store URL used by the registration/licensing flow |
| `SerialNumber` | Plugin serial/licensing value |

## DPD API integration

The plugin uses the DPD API for:

- authentication and session creation
- pickup location search
- shipment-related request models

The pickup-point search is based on a nearby-address lookup and is tailored around postcode-driven selection. The current frontend validation is geared toward UK postcode entry.

## Authentication type

The plugin uses **Basic Authentication plus a session-based token flow**.

It is **not OAuth2** and does not use bearer JWT tokens.

## How authentication works

The plugin authenticates against the DPD API using the credentials entered in the admin configuration:

- DPD username
- DPD password
- DPD account number
- configured DPD base URL

The authentication flow is:

1. The plugin sends a login request to the DPD API using the configured credentials.
2. DPD returns a session value named `geoSession`.
3. The plugin stores that `geoSession` in a central cache through `IDPDSessionService` / `DPDSessionService`.
4. Subsequent pickup-point and shipment requests ask the session service for the current token instead of performing their own login.
5. Those requests include:
   - a Basic `Authorization` header built from the DPD username and password
   - a `GEOClient` header using the DPD account number
   - a `GEOSession` header using the returned session token
6. If the cached token is missing or expired, the session service performs one fresh login and updates the cache.

In short, the auth model is **Basic Auth + session token headers**.

## GeoSession reuse

The plugin now reuses the DPD `geoSession` instead of logging in before every downstream API call.

This behavior is intentional and is handled centrally by `Services/DPDSessionService.cs`:

- `GetGeoSessionAsync(...)` returns the cached session when it is still valid
- `RefreshGeoSessionAsync(...)` forces a fresh DPD login and replaces the cached session
- the cached value is stored as an `AccessTokenItem` in distributed cache
- the current implementation gives the cached `geoSession` a 1-day sliding expiration window

This reduces unnecessary DPD login calls and keeps pickup-point lookup and shipment creation on the same session-management path.

`DPDShipToShopClient` is now token-driven for pickup-point and shipment requests. It no longer performs an internal login for those operations; instead, callers obtain the current `geoSession` first and pass it into the client configuration.

## Google Maps integration

The public widget relies on Google Maps to:

- render the pickup-point map
- display markers for shops
- support optional autocomplete lookup
- show a ranked list of nearby pickup locations

If the Google Maps API key is not configured, the pickup-point UI will not render as intended.

## Order notes and email token

When a DPD Ship to Shop order is placed, the plugin stores the selected location and adds it to the order details context.

A custom message token is also registered:

- `%Order.DPDShipToShopLocation%`

Add this token manually to your nopCommerce email templates if you want the selected pickup-point address to appear in order-related emails.

## Important behavior to know

- The plugin is **self-contained** and avoids direct nopCommerce core changes.
- The shipping method name match is important; the configured value must align with the name shown at checkout.
- The pickup-point UI is shown on the checkout shipping-method step, not as a standalone shipping provider screen.
- The integration is oriented toward DPD pickup-shop delivery workflows and postcode-based search.

## Project structure

Key areas of the repository:

- `Components/` – widget view components for checkout and admin buttons
- `Controllers/` – configuration, pickup-point, and admin shipment endpoints
- `Services/` – persistence, event handling, licensing, and message token logic
- `Views/` – admin and public UI views
- `Scripts/` – map and pickup-point interaction logic
- `Themes/` – plugin styles and marker images
- `Data/` and `Domain/` – schema and entity models for stored pickup locations

## Demo

Video walkthrough:

- https://www.youtube.com/watch?v=bCMCrsg9Myg

## Summary

This repository provides a practical DPD Ship to Shop integration for nopCommerce 4.50, combining checkout pickup-point selection, map-based search, saved location data, and admin-side order handling in a single plugin.
