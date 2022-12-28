# Rogue.Contacts

## PLEASE READ

The project was scrapped due to the size of the application scope and the time needed to finish it.  
But non the less, I have learned a lot from it which was the initial intention when starting the project.

### There are a couple of problems with the code base:

Firstly I'll begin with the database model.  
The Roles and Permissions tables are duplicated which isn't very neat especially if you are looking to add another feature.  
Looking back at it with the newly gained experience I should have done the following in this situation:
- Used a string primary key containing the owner name and business name for the businesses and used the organization name as the primary key for the organization.
- In the case of business/organization name change then I should recreate the business/organization and either remove the old database entry completely or leave some information to use it for redirection to the new name.
- Have a single Roles table and a single permissions table with a single joining table. The users to roles joining table would consist of 3 primary keys, a foreign key to User, a foreign key to the Role and a foreign key to the new Business/Organization primary key which can be solved in multiple of ways, one would be using a hierarchy.

Second issue was the fact that the Service methods did a lot of work.  
Granted that work did indeed need to happen in the service layer, because the service layer should be agnostic to the top level application that is using it.  
There are a few solutions I could have done to improve this:
- I could have refactored the permission checking to be a method which takes in params of a value type consisting of two properties:
    - The permission enum (which would specify what permission to check for)
    - An error (which would specify what error to return when the user does not have the permission)
    - The resource name (if we applied the above database changes)
- Additionally If I wanted to I could have used MediatR library to implement middleware in the service layer and used pipelines for both validation and authorization which would get rid of all repetitive validation and permission checking code inside the service methods.

Third issue that I have found out is the fact that Web Api layer should have its own dtos for the parameters from body.  
The parameters in an endpoint could come from multiple points, for example from the url query parameters, from the body or from the header. To maintain consistency on body parameters through out all endpoints I should create a dto containing the body, even if there is only 1 property in them so that all requests expect a object inside the body containing the paramaters.  
Another reason it should have its own dtos is the aspect of versioning, Web Api endpoints should not change. If I changed a dto in the service layer that would essentially change the endpoints if I am re-using the same dtos from the service layer.

## Conclusion

Overall I learned a lot of things when encountering issues in this project even some that I didn't even note down here! I am now confident that I can architect an even better solution next time around!

### The learning path of a developer never ends, but we get closer and closer with each project we are involved in!  
#### Best regards!
