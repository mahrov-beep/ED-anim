#!/bin/bash

echo "Uploading IPA to Appstore Connect..."

path="$WORKSPACE/.build/last/$BUILD_TYPE/build.ipa"

if xcrun altool --upload-app --type ios -f $path -u $ITUNES_USERNAME -p $ITUNES_PASSWORD ; then
    echo "Upload IPA to Appstore Connect finished with success"
else
    echo "Upload IPA to Appstore Connect failed"
fi