# DomainSearch
Simple Windows app for collecting availability information and best offers for domains

https://user-images.githubusercontent.com/22728934/213916926-3717e566-15ef-4f3f-ac95-67cab7b2d287.mp4

### Setup
1. Clone this repository
2. Open the .sln file using Visual Studio.
3. Set a location for the SQLite database file in the DomainSearch.Data/Models.cs file at line 13.
4. Update the list of domains and TLDs you want to check in DomainSearch/ViewModel.cs starting at line 35.
5. Open a *single* Chrome window and go to https://domainr.com. Make sure the browser's display language is set to English.
6. Start the DomainSearch project. If the app doesn't open, search for Configuration Manager and make sure Deploy is checked.

### Usage
1. Search for a domain on domainr.com.
2. Select one of the options.
3. The app should show the selected domain under "Domain:".
4. If the domain is not available uncheck "Is available?" and press Save.
5. If the domain is available click on one of the registrars on Domainr or search for other registrars on the internet.
6. Once you are on a registrars page the app should show the registrar's address under "Registrar:".
7. Check "Is available?" and fill out "$/yr" with the amount the registrar asks for per year in dollars.
8. Press Save. The registrar's address and price should show up under "Offers:".
9. Now you can check offers by other registrars or search for a different domain.
10. You can see how much the different registrars usually ask for the TLD of the current domain under "TLD stats".

### Using the collected data
Everything is stored in the SQLite database. Check out the DomainSearch.Console project for an example on how to run queries on it using Entity Framework.
