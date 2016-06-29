if [ -z ${1+x} ]
then
    PACK_MODE="Release"
else 
    PACK_MODE=$1
fi
SOLUTION_NAME="NinjaTurtlesMutation.sln"
NUGET_CORE_DIR="NinjaTurtlesMutation"
NUGET_CORE_PROJ="NinjaTurtlesMutation.csproj"
TEMP_NUGET_DIR="nuget_temp"

YELLOW='\033[1;33m'
BLUE='\033[1;34m'
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m'


function checked_cmd() { 
    output="$(eval $1 2>&1)"
    exit_code=$?

    if [ $exit_code != 0 ]
    then
        printf "${BLUE}$2${NC} ${RED}[FAIL]${NC}\n"
        echo "$1 --->"
        echo "$output"
    else
        printf "${BLUE}$2${NC} ${GREEN}[OK]${NC}\n"
    fi
    if [ $exit_code != 0 ] && [ $3 == 1 ]
    then
        echo "Exit!"
        exit 1
    fi
}

function MSB_clean() {    
    cmd="MSBuild.exe -p:Configuration=$1 /target:clean $SOLUTION_NAME"
    checked_cmd "$cmd" "Clean $1" 0
}

function MSB_build() {
    cmd="MSBuild.exe -p:Configuration=$1 $SOLUTION_NAME" 
    checked_cmd "$cmd" "Build $1" 1
}

function Nuget_pack() {
    cmd="nuget pack -Verbosity detailed -NoDefaultExcludes -Prop Configuration=$1 -OutputDirectory $TEMP_NUGET_DIR $NUGET_CORE_PROJ"
    checked_cmd "$cmd" "Nuget packing $1" 1
}

function Nuget_push() {
    cmd="nuget push $1 -Source https://www.nuget.org/api/v2/package"
    prompted_cmd "$cmd" "Pushing package to nuget repo"
}

function prompted_cmd() {
    cmd=$1
    warn_msg="About to execute: $2"
    printf "${YELLOW}$warn_msg${NC}\n"
    while true; do
        read -p "Proceed ? " -r
        echo
        case $REPLY in
            [Yy]* ) checked_cmd "$cmd" "$2" 1; break;;
            [Nn]* ) printf "${YELLOW}Skipping $2\n($cmd)${NC}\n"; break;;
            * ) echo "Please answer yes or no.";;
        esac
    done
}



MSB_clean Debug
MSB_clean Release

MSB_build $PACK_MODE

cd $NUGET_CORE_DIR
if [ $? != 0 ]
then
    echo "Exit!"
    exit 1
fi

checked_cmd "rm -rf $TEMP_NUGET_DIR && mkdir $TEMP_NUGET_DIR" "Reset nuget directory" 1
Nuget_pack $PACK_MODE
Nuget_push "$TEMP_NUGET_DIR/*.nupkg"
printf "${GREEN}Bye.${NC}\n"

