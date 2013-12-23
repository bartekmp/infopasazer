infopasazer
===========

C# interface for PKP PLK InfoPasażer online browser.

This written in WPF and .NET 4.5 app for Windows uses HtmlAgilityPack & LINQ for parsing poorly designed (and also coded) site http://infopasazer.intercity.pl.
That site is made by PKP Intercity S.A. - polish railroad operator and provides information about trains and their routes in real time.
Though the site is awful, the goal of this project is to deliver quite good-looking, simple and relatively fast interface to search InfoPasażer data without using it's website.

I realise, that many elements of that are now just poor written, but it's still just a pre-alfa :).
Even though I think, that the app covers main functionality of InfoPasażer.

InfoPasażer is said to gather data from these railroad companies:

      PKP Intercity S.A.	
      PKP Polskie Linie Kolejowe S.A.	
      Arriva Sp. z o.o.	
      Koleje Dolnośląskie Sp. z o.o.	
      Koleje Mazowieckie - KM Sp. z o.o.	
      Koleje Śląskie Sp. z o.o.	
      Koleje Wielkopolskie Sp. z o.o.	
      Przewozy Regionalne Sp. z o.o.	
      SKM Trójmiasto Sp. z o.o.	
      SKM Warszawa Sp. z o.o.
      
If you want to compile source of that project, you're going to need these references:
      HtmlAgilityPack 1.4.6
      System.Net.Http
      System.Web.RegularExpressions
      System.Xml.Linq
      
To run the app, you'll need .NET 4.5 framework.

This application is under Creative Commons BY-NC-SA 3.0 license.
Feel free to clone & edit source, but don't forget about comments about author (bartekmp) and redistributing it under the same license!
