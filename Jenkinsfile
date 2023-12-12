pipeline{
    
    agent any
    environment {
        registry = "excellerentsolutions/eppbe"
        registryCredential = 'dockerhubID'
        eppbeImage = ''
        IMAGE_TAG = "excellerentsolutions/eppbe:latest"
        
    }
    
    stages
    { 
        stage('Git checkout')
        {
            steps{
              git credentialsId: 'jenkins-bitbucket-omeseret', url: 'https://bitbucket.org/Excellerent_Solutions/excellerent-epp-be'
        
            }
        }
        stage('Dotnet build')
        {
            agent{
             docker
                {
                 image 'mcr.microsoft.com/dotnet/sdk:5.0'
                  args '-u root:root'
                }
            }
            steps{
               sh 'dotnet build' 
            }
        }
        stage('Dotnet test')
        {
            agent{
             docker
                {
                 image 'mcr.microsoft.com/dotnet/sdk:5.0'
                  args '-u root:root'
                }
            }
            steps{
              sh 'dotnet test'  
            }
        }
        stage('Deploy to Staging')
        {
            when {
                branch 'master'
            }
            steps{
                script 
                {
                
                 
                    sshagent(credentials : ['dev']) {
                        eppbeImage = docker.build registry + (":latest")
                        docker.withRegistry( '', 'dockerhubID' )
                            {
                             eppbeImage.push()
                                 
                  sh "rsync -rv --delete -e 'ssh' ./docker-compose.yml ubuntu@3.138.163.97:/home/ubuntu/deployment"  
                  
                  sh "ssh -o StrictHostKeyChecking=no  ubuntu@3.138.163.97 sudo docker-compose -f /home/ubuntu/deployment/docker-compose.yml down"
                  sh "ssh -o StrictHostKeyChecking=no  ubuntu@3.138.163.97 sudo docker system prune -af"
                  sh "ssh -o StrictHostKeyChecking=no  ubuntu@3.138.163.97 sudo docker-compose -f /home/ubuntu/deployment/docker-compose.yml up -d "}
                            }
                      
                    }
              //clean the workspace after deployment
                cleanWs deleteDirs: true, notFailBuild: true
            }     
                 
            
        }
    
    }
}
