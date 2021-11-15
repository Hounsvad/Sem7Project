from pgmagick import Image
import sys

img = Image(sys.argv[1]) # Input Image
img.write(sys.argv[2])  # Output Image