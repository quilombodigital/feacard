# feacard

My daughter needed an entrance card generator for the USP FEA Economics course. I already did a label generator for her (another repository), so I decided to make another one more generic, that could be "programmed" to do other things by using a config file.

You just need to create a json file with the visual layout (definition) of what you want printed, and a CSV file with all the dynamic data you want to be inserted.
The app supports to automatically find faces in images and crop correctly the image. This is very usefull because the students are going to send their selfies. The system also supports image urls, and the system will download them.

You can download the current version here: [FeaCard Version 1.0.1](https://github.com/quilombodigital/feacard/releases/download/v1.0.1/feacard-1.0.1.zip)

You can download the examples here: [Examples](https://github.com/quilombodigital/feacard/releases/download/v1.0.1/feacard-examples-1.0.1.zip)

## Instructions:
* Modify the file **definition.json**
* Modify the file **data.csv**
* Execute **FeaCard.exe**



![image](https://user-images.githubusercontent.com/874378/173989370-bd975131-6bb2-40a4-a6a3-698e265b337e.png)

Example Definition File
![image](https://user-images.githubusercontent.com/874378/173989773-3af4a732-ab51-4c42-a663-227392ae74f2.png)

Example CSV file
![image](https://user-images.githubusercontent.com/874378/173989857-85a59684-dbbd-497b-8b48-d10623b28127.png)


Result
![image](https://user-images.githubusercontent.com/874378/173989467-e756b99a-729f-4ace-aa7b-4575d61f47b5.png)

![image](https://user-images.githubusercontent.com/874378/173989500-a4b9bb66-c91d-4d68-a4d2-6a545af4a31c.png)

![image](https://user-images.githubusercontent.com/874378/173989595-73fc0752-81d5-40f0-b388-538e8b6444db.png)

