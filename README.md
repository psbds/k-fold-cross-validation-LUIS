# k-fold-cross-validation-LUIS

K-fold cross-validation for LUIS Models (https://en.wikipedia.org/wiki/Cross-validation_(statistics)#k-fold_cross-validation)

### System Requeriments
* dotnet core (https://www.microsoft.com/net/learn/get-started/)


### How to Run
* Clone the repository and go to the solution folder
* Run ```dotnet build```
* Run ```dotnet run --project Psbds.LUIS.Experiment.Console\Psbds.LUIS.Experiment.Console.csproj```
* Provide your applicationKey, applicationId, version name to be tested and directory to save the result files 

### Results
   1. The Console will display the accuracy for each fold and the average accuracy
   2. The ConfusionMatrix File
        * Nº of Confusions;Intent;Confusion Intents(Count)
   3. The Utterances File
        * Correct;Utterance;Expected Intent;FirstIntent;Second Intent
