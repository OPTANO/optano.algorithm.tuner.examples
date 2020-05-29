#!/bin/bash
ulimit -m $2
echo ulimit -m: $(ulimit -m)
ulimit -v $2
echo ulimit -v: $(ulimit -v)
echo $1
exec $1
