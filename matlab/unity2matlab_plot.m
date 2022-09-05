% recorded data .txt file format to plot
% Unity sends vehicle position data x, y, heading angle
% float under 4 decimals is included in string format with space
% example: 1.1111 2.0000 3.2222

% read txt file and convert in to table data
T = readtable("vehicle_status.txt");
status = table2array(T);

% for color information check -> http://www.n2n.pe.kr/lev-1/color.htm

%% trajectory
title("trajectory of mobile robot", 'fontweight', 'bold');
plot(status(:,1), status(:,2), 'Color','#9966ff', 'LineWidth', 2);
grid on;
xlim([-1 1]); ylim([-1 1]);

%% yaw angle
title("heading angle of mobile robot", 'fontweight', 'bold');
plot(status(:,4), status(:,3), 'Color','#9966ff', 'LineWidth', 2);
grid on;
xlim([0 status(end,4)]); ylim([min(status(:,3)) max(status(:,3))]);

%% rpm
title("left and right wheel rotation velocity result", 'fontweight', 'bold');
subtitle("reference = 10[rad/s]");
plot(status(:,4), status(:,1), '-.','Color','#ff00cc', 'LineWidth', 2); hold on;
plot(status(:,4), status(:,2), 'Color','#3333cc', 'LineWidth', 2);
plot([10:1:15], 10*ones(length([10:1:15])),'g--','LineWidth', 3);
legend('left wheel', 'right wheel', 'reference=10 [rad/s]');
xlabel('[seconds]'); ylabel('[rad/s]');
xlim([10 15]); ylim([-30 40]);
grid on;
hold off;