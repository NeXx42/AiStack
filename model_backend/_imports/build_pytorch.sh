#!/usr/bin/env bash
# this has to be the worst thing i have ever done.... DO NOT TOUCH THE VARS
# run "python setup.py install" after built (needs enviroment vars)

set -e
source ../.env/bin/activate

cd ./pytorch

# ROCm root
export ROCM_PATH=/opt/rocm-7.1.0
export PATH=$ROCM_PATH/bin:$PATH
export LD_LIBRARY_PATH=$ROCM_PATH/lib:$ROCM_PATH/lib64:$LD_LIBRARY_PATH
export CPATH=$ROCM_PATH/include:$CPATH
export CMAKE_PREFIX_PATH=$ROCM_PATH:$CMAKE_PREFIX_PATH
export HIP_PATH=$ROCM_PATH
export HIP_PLATFORM=amd


export PYTORCH_ROCM_ARCH=gfx1101


rm -rf build
python setup.py clean

python setup.py build

deactivate