import React from 'react';
import Moment from 'react-moment';
import moment from 'moment';

export const formatDifference = (now, then) => moment(moment(then).diff(moment(now))).format('mm:ss');

export const showDate = (date, fallback) => {
  if (date) {
    return <Moment format='YYYY-MM-DD HH:mm'>{date}</Moment>
  }

  return fallback;
}
