import sys

# argv: instance_name 0 cutoff_time_in_s 0 unsigned_seed -parameter_id_1 value_1 -parameter_id_2 value_2
def main(argv):
   firstNumber = float(argv[7])
   secondNumber = float(argv[9])
   result = firstNumber + secondNumber
   # output: status, runtime, runlegth, quality, seed
   print("Result for ParamILS: Sat, 0, 0, %f, 42" % result)

if __name__ == "__main__":
   main(sys.argv[0:])