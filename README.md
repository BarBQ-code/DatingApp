# DatingApp
Things you need to have before running the project:
1: .NET 5.0 installed
2: SqlServer installed
3: Angular and npm if you want to run it manually

Configuartion:
The app uses an extenral cloud service to store the images, so You need to register an account in cloudinary, it's free and doesn't require a credit card. 
Register url: https://cloudinary.com/users/register/free
Next, go to the appsetting.json and get Fill in the required Cloudinary settings which include: CloudName, ApiKey, ApiSecret, you can get them in the cloudinary dashboard after you register.

Now you can run the project, to run in open the project folder in the terminal and cd into the API folder and run the following command:
```dotnet watch run```
Now a webserver should start, you can access the web app through: https://localhost:5001
(note: it's important you enter the app through https and port 5001 rather than normal http and port 5000)

There is seed data injected to the app everetime the database doens't exist so it feels more real and intuitive
You can find the list of users in API/Data/UserSeedData.json

All the users have the same password which is: Pa$$w0rd 
and there is a special admin user which also password is: Pa$$w0rd, and it has special privilages

e.g for a user:
Username: clay
Password: Pa$$w0rd
