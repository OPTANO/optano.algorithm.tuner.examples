import bbobbenchmarks as bn
import numpy as np
import sys

# argv: functionId, instanceId, [parameters to evaluate]
def main(argv):
   funId = int(argv[0])
   instance = int(argv[1])
   funcArgs = argv[2:]
   parameters = np.array(funcArgs, dtype=float)
   funcInstance = bn.instantiate(funId, instance)
   function = funcInstance[0]
   optVal = funcInstance[1]
   
   result = function(parameters)
   deltaOpt = (result - optVal)
   output = "result=%f" % deltaOpt
   print output

if __name__ == "__main__":
   main(sys.argv[1:])
