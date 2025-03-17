# Imposta il percorso del progetto e la cartella di output
$projectPath = "./Lapo.Mud/Lapo.Mud.csproj"
$outputPath = "./dist"

# Esegui il comando di pubblicazione in modalità self-contained
dotnet publish $projectPath -c Release -o $outputPath --self-contained true