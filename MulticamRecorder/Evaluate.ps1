param (
	[string]$dir = ".",
	[long]$frq = 2825673 # ticks/sec
)

$timestamps =  (gci $dir | % {$_.BaseName.SubString(4)} | sort )
$num_of_ts = $timestamps.Count
$before = $timestamps[0..($num_of_ts-2)]
$after = $timestamps[1..$num_of_ts]
$num_of_diff = $num_of_ts-1

$sum = [long]0;
for ($i=0; $i -lt $num_of_diff; $i++ ) {
	$diff = [long]$after[$i]-[long]$before[$i];
	$diffs+= @($diff)
	$sum+=$diff
}

$mean = ([double]$sum)/$num_of_diff # tick

$time = ([double]([long]$timestamps[-1] - [long]$timestamps[0]))/$frq # sec
$fps = ([double]$timestamps.Count)/$time # fps

$sum = [double]0;
for ($i=0; $i -lt $num_of_diff; $i++ ) {
	$dev = ([double]$diffs[$i])-$mean;
	$sum+=$dev*$dev
}

$variance = $sum/($num_of_diff-1) # tick^2

$deviance = [System.Math]::Sqrt($variance)*1000/$frq # ms

echo ("Duration: {0} sec" -f $time)
echo ("Frames: {0}" -f $num_of_ts)
echo ("Frequency: {0} fps" -f $fps) 
echo ("Mean: {0} ms" -f ($mean*1000/$frq))
echo ("Dev: {0} ms" -f $deviance)
