from pgmagick import Image
import sys

print(f'Converting {sys.argv[1]} to {sys.argv[2]}')
img = Image(sys.argv[1]) # Input Image
img.write(sys.argv[2])  # Output Image
print('Conversion complete')