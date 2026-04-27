pipeline {
    agent { docker { image 'mcr.microsoft.com/dotnet/sdk:9.0' } }
    environment {
        NEXUS_CREDS         = credentials('NEXUS_CREDENTIALS')
        NEXUS_NUGET_API_KEY = credentials('NEXUS_NUGET_API_KEY')
        NEXUS_PUSH_URL      = 'https://nexus.your-org.com/repository/nuget-internal/'
        NEXUS_PULL_URL      = 'https://nexus.your-org.com/repository/nuget-group/index.json'
        GIT_COMMIT_SHORT    = sh(script: 'git rev-parse --short HEAD', returnStdout: true).trim()
        GIT_BRANCH_CLEAN    = sh(script: 'echo "$GIT_BRANCH" | sed "s|origin/||;s|/|-|g"', returnStdout: true).trim()
    }
    stages {
        stage('Prepare') {
            steps {
                script {
                    env.VERSION = sh(script: 'dotnet tool run minver --tag-prefix v --default-pre-release-phase alpha', returnStdout: true).trim()
                    currentBuild.displayName = "#${BUILD_NUMBER} · ${VERSION} · ${GIT_COMMIT_SHORT}"
                }
                sh 'dotnet nuget add source "${NEXUS_PULL_URL}" --name nexus --username "${NEXUS_CREDS_USR}" --password "${NEXUS_CREDS_PSW}" --store-password-in-clear-text --configfile ./nuget.config'
            }
        }
        stage('Restore')  { steps { sh 'dotnet restore --locked-mode' } }
        stage('Build')    { steps { sh 'dotnet build --no-restore --configuration Release -p:ContinuousIntegrationBuild=true' } }
        stage('Test')     {
            steps { sh 'dotnet test --no-build --configuration Release --logger "trx;LogFileName=results.trx" --results-directory ./TestResults' }
            post  { always { mstest testResultsFile: '**/TestResults/*.trx', keepLongStdio: true } }
        }
        stage('Pack') {
            steps {
                sh 'dotnet pack --no-build --configuration Release --output ./nupkgs -p:ContinuousIntegrationBuild=true -p:RepositoryBranch="${GIT_BRANCH_CLEAN}" -p:RepositoryCommit="${GIT_COMMIT_SHORT}"'
                archiveArtifacts artifacts: 'nupkgs/*.nupkg,nupkgs/*.snupkg', fingerprint: true
            }
        }
        stage('Publish') {
            when { anyOf { branch 'main'; tag pattern: 'v\\d+\\.\\d+\\.\\d+', comparator: 'REGEXP' } }
            steps {
                sh 'dotnet nuget push ./nupkgs/*.nupkg --api-key "${NEXUS_NUGET_API_KEY}" --source "${NEXUS_PUSH_URL}" --skip-duplicate'
                sh 'dotnet nuget push ./nupkgs/*.snupkg --api-key "${NEXUS_NUGET_API_KEY}" --source "${NEXUS_PUSH_URL}" --skip-duplicate'
            }
        }
    }
    post {
        always { sh 'dotnet nuget remove source nexus --configfile ./nuget.config || true' }
        success { script { currentBuild.description = "NuGet: ${VERSION}" } }
    }
}
