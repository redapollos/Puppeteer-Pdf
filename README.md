# Puppeteer-Pdf

This Azure function will turn any url into a PDF.  Created as an example.  Fork and change to your heart's desire.

Clone this repo and run locally.  

Deploy to Azure Function in Linux using the consumption plan.

Use the following query string parameters to customize it:
url -url you want to turn into a pdf - mandatory
w - width
m - margin
n - name
pb - print background

https://yoursite.azurewebsites.net/api/GeneratePdf?code=whateveryoursecuritycodeis&url=somesite.com
https://yoursite.azurewebsites.net/api/GeneratePdf?code=whateveryoursecuritycodeis&url=somesite.com&w=1500
https://yoursite.azurewebsites.net/api/GeneratePdf?code=whateveryoursecuritycodeis&url=somesite.com&m=25
https://yoursite.azurewebsites.net/api/GeneratePdf?code=whateveryoursecuritycodeis&url=somesite.com&n=mydownload
https://yoursite.azurewebsites.net/api/GeneratePdf?code=whateveryoursecuritycodeis&url=somesite.com&pb=false
https://yoursite.azurewebsites.net/api/GeneratePdf?code=whateveryoursecuritycodeis&url=somesite.com&w=800&m=25&n=mydownload&pb=false
